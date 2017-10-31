// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Protocols;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets
{
    internal sealed class SocketConnection : TransportConnection
    {
        private const int MinAllocBufferSize = 2048;

        private readonly Socket _socket;
        private readonly ISocketsTrace _trace;

        private readonly ThreadPoolBoundHandle _threadPoolBoundHandle;
        private readonly PreAllocatedOverlapped _receiveOverlapped;
        private readonly PreAllocatedOverlapped _sendOverlapped;
        private readonly SocketAwaitable _receiveAwaitable;
        private readonly SocketAwaitable _sendAwaitable;

        private bool _skipCompletionPortOnSuccess;
        private volatile bool _aborted;

        internal SocketConnection(Socket socket, PipeFactory pipeFactory, ISocketsTrace trace)
        {
            Debug.Assert(socket != null);
            Debug.Assert(pipeFactory != null);
            Debug.Assert(trace != null);

            _socket = socket;
            _receiveAwaitable = new SocketAwaitable();
            _sendAwaitable = new SocketAwaitable();
            _threadPoolBoundHandle = ThreadPoolBoundHandle.BindHandle(new UnownedSocketHandle(socket));
            _receiveOverlapped = new PreAllocatedOverlapped(_receiveCallback, state: this, pinData: null);
            _sendOverlapped = new PreAllocatedOverlapped(_sendCallback, state: this, pinData: null);

            PipeFactory = pipeFactory;
            _trace = trace;

            var localEndPoint = (IPEndPoint)_socket.LocalEndPoint;
            var remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;

            LocalAddress = localEndPoint.Address;
            LocalPort = localEndPoint.Port;

            RemoteAddress = remoteEndPoint.Address;
            RemotePort = remoteEndPoint.Port;
        }

        public override PipeFactory PipeFactory { get; }
        public override IScheduler InputWriterScheduler => InlineScheduler.Default;
        public override IScheduler OutputReaderScheduler => InlineScheduler.Default;

        public async Task StartAsync(IConnectionHandler connectionHandler)
        {
            try
            {
                connectionHandler.OnConnection(this);

                try
                {
                    _skipCompletionPortOnSuccess = SetFileCompletionNotificationModes(_socket.Handle,
                        FileCompletionNotificationModes.SkipCompletionPortOnSuccess | FileCompletionNotificationModes.SkipSetEventOnHandle);
                }
                catch
                {
                }

                // Spawn send and receive logic
                Task receiveTask = DoReceive();
                Task sendTask = DoSend();

                // If the sending task completes then close the receive
                // We don't need to do this in the other direction because the kestrel
                // will trigger the output closing once the input is complete.
                if (await Task.WhenAny(receiveTask, sendTask) == sendTask)
                {
                    // Tell the reader it's being aborted
                    _socket.Dispose();
                }

                // Now wait for both to complete
                await receiveTask;
                await sendTask;

                // Dispose the socket(should noop if already called)
                _socket.Dispose();
                _threadPoolBoundHandle.Dispose();
                _receiveOverlapped.Dispose();
                _sendOverlapped.Dispose();
            }
            catch (Exception ex)
            {
                _trace.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}.");
            }
        }

        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    // Ensure we have some reasonable amount of buffer space
                    var buffer = Input.Alloc(MinAllocBufferSize);

                    try
                    {
                        var bytesReceived = await ReceiveAsync(buffer.Buffer);

                        if (bytesReceived == 0)
                        {
                            // FIN
                            _trace.ConnectionReadFin(ConnectionId);
                            break;
                        }

                        buffer.Advance(bytesReceived);
                    }
                    finally
                    {
                        buffer.Commit();
                    }

                    var flushTask = buffer.FlushAsync();

                    if (!flushTask.IsCompleted)
                    {
                        _trace.ConnectionPause(ConnectionId);

                        await flushTask;

                        _trace.ConnectionResume(ConnectionId);
                    }

                    var result = flushTask.GetAwaiter().GetResult();
                    if (result.IsCompleted)
                    {
                        // Pipe consumer is shut down, do we stop writing
                        break;
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                error = new ConnectionResetException(ex.Message, ex);
                _trace.ConnectionReset(ConnectionId);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted ||
                                             ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                             ex.SocketErrorCode == SocketError.Interrupted ||
                                             ex.SocketErrorCode == SocketError.InvalidArgument)
            {
                if (!_aborted)
                {
                    // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
                    error = new ConnectionAbortedException();
                    _trace.ConnectionError(ConnectionId, error);
                }
            }
            catch (ObjectDisposedException)
            {
                if (!_aborted)
                {
                    error = new ConnectionAbortedException();
                    _trace.ConnectionError(ConnectionId, error);
                }
            }
            catch (IOException ex)
            {
                error = ex;
                _trace.ConnectionError(ConnectionId, error);
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
                _trace.ConnectionError(ConnectionId, error);
            }
            finally
            {
                if (_aborted)
                {
                    error = error ?? new ConnectionAbortedException();
                }

                Input.Complete(error);
            }
        }

        private async Task DoSend()
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    // Wait for data to write from the pipe producer
                    var result = await Output.ReadAsync();
                    var buffer = result.Buffer;

                    if (result.IsCancelled)
                    {
                        break;
                    }

                    try
                    {
                        if (!buffer.IsEmpty)
                        {
                            await SendAsync(buffer);
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        Output.Advance(buffer.End);
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                error = null;
            }
            catch (ObjectDisposedException)
            {
                error = null;
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                Output.Complete(error);

                // Make sure to close the connection only after the _aborted flag is set.
                // Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
                // a BadHttpRequestException is thrown instead of a TaskCanceledException.
                _aborted = true;
                _trace.ConnectionWriteFin(ConnectionId);
                _socket.Shutdown(SocketShutdown.Both);
            }
        }

        private unsafe SocketAwaitable ReceiveAsync(Buffer<byte> buffer)
        {
            var overlapped = _threadPoolBoundHandle.AllocateNativeOverlapped(_receiveOverlapped);

            buffer.TryGetArray(out var bufferSegment);
            fixed (byte* bufferPtr = &bufferSegment.Array[bufferSegment.Offset])
            {
                var wsaBuffer = new WSABuffer
                {
                    Length = buffer.Length,
                    Pointer = (IntPtr)bufferPtr
                };

                var socketFlags = SocketFlags.None;
                var errno = WSARecv(
                    _socket.Handle,
                    &wsaBuffer,
                    1,
                    out var bytesTransferred,
                    ref socketFlags,
                    overlapped,
                    IntPtr.Zero);

                errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

                if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
                {
                    _threadPoolBoundHandle.FreeNativeOverlapped(overlapped);
                    _receiveAwaitable.Complete(bytesTransferred, errno);
                }
            }

            return _receiveAwaitable;
        }

        private unsafe SocketAwaitable SendAsync(ReadableBuffer buffers)
        {
            if (buffers.IsSingleSpan)
            {
                return SendAsync(buffers.First);
            }

            var bufferCount = (int)buffers.Length;
            var wsaBuffers = stackalloc WSABuffer[bufferCount];

            var i = 0;
            foreach (var buffer in buffers)
            {
                wsaBuffers[i].Length = buffer.Length;

                buffer.TryGetArray(out var bufferSegment);
                fixed (byte* bufferPtr = &bufferSegment.Array[bufferSegment.Offset])
                {
                    wsaBuffers[i].Pointer = (IntPtr)bufferPtr;
                }

                i++;
            }

            var overlapped = _threadPoolBoundHandle.AllocateNativeOverlapped(_sendOverlapped);

            var errno = WSASend(
                _socket.Handle,
                wsaBuffers,
                bufferCount,
                out var bytesTransferred,
                SocketFlags.None,
                overlapped,
                IntPtr.Zero);

            errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

            if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
            {
                _threadPoolBoundHandle.FreeNativeOverlapped(overlapped);
                _sendAwaitable.Complete(bytesTransferred, errno);
            }

            return _sendAwaitable;
        }

        private unsafe SocketAwaitable SendAsync(Buffer<byte> buffer)
        {
            var overlapped = _threadPoolBoundHandle.AllocateNativeOverlapped(_sendOverlapped);

            buffer.TryGetArray(out var bufferSegment);
            fixed (byte* bufferPtr = &bufferSegment.Array[bufferSegment.Offset])
            {
                var wsaBuffer = new WSABuffer
                {
                    Length = buffer.Length,
                    Pointer = (IntPtr)bufferPtr
                };

                var errno = WSASend(
                    _socket.Handle,
                    &wsaBuffer,
                    1,
                    out var bytesTransferred,
                    SocketFlags.None,
                    overlapped,
                    IntPtr.Zero);

                errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

                if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
                {
                    _threadPoolBoundHandle.FreeNativeOverlapped(overlapped);
                    _sendAwaitable.Complete(bytesTransferred, errno);
                }
            }

            return _sendAwaitable;
        }

        private static unsafe readonly IOCompletionCallback _receiveCallback = new IOCompletionCallback(ReceiveCompletionCallback);
        private static unsafe readonly IOCompletionCallback _sendCallback = new IOCompletionCallback(SendCompletionCallback);

        private static unsafe void ReceiveCompletionCallback(uint errno, uint bytesTransferred, NativeOverlapped* nativeOverlapped)
        {
            var socketConnection = (SocketConnection)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped);

            var socketError = GetSocketError(socketConnection, nativeOverlapped, errno);

            socketConnection._threadPoolBoundHandle.FreeNativeOverlapped(nativeOverlapped);
            socketConnection._receiveAwaitable.Complete((int)bytesTransferred, socketError);
        }

        private static unsafe void SendCompletionCallback(uint errno, uint bytesTransferred, NativeOverlapped* nativeOverlapped)
        {
            var socketConnection = (SocketConnection)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped);

            var socketError = GetSocketError(socketConnection, nativeOverlapped, errno);

            socketConnection._threadPoolBoundHandle.FreeNativeOverlapped(nativeOverlapped);
            socketConnection._sendAwaitable.Complete((int)bytesTransferred, socketError);
        }

        // https://github.com/dotnet/corefx/blob/a26a684033c0bfcfbde8e55c51b8b03fb7a3bafc/src/System.Net.Sockets/src/System/Net/Sockets/BaseOverlappedAsyncResult.Windows.cs#L61
        private static unsafe SocketError GetSocketError(SocketConnection connection, NativeOverlapped* nativeOverlapped, uint errno)
        {
            // Complete the IO and invoke the user's callback.
            var socketError = (SocketError)errno;

            if (socketError != SocketError.Success && socketError != SocketError.OperationAborted)
            {
                // There are cases where passed errorCode does not reflect the details of the underlined socket error.
                // "So as of today, the key is the difference between WSAECONNRESET and ConnectionAborted,
                //  .e.g remote party or network causing the connection reset or something on the local host (e.g. closesocket
                // or receiving data after shutdown (SD_RECV)).  With Winsock/TCP stack rewrite in longhorn, there may
                // be other differences as well."
                if (connection._aborted)
                {
                    socketError = SocketError.OperationAborted;
                }
                else
                {
                    // The async IO completed with a failure.
                    // Here we need to call WSAGetOverlappedResult() just so GetLastSocketError() will return the correct error.
                    bool success = WSAGetOverlappedResult(
                        connection._socket.Handle,
                        nativeOverlapped,
                        out _,
                        false,
                        out _);

                    if (!success)
                    {
                        socketError = (SocketError)Marshal.GetLastWin32Error();
                    }
                    else
                    {
                        Trace.Assert(false, $"Unexpectedly succeeded. errorCode: '{errno}'");
                    }
                }
            }

            return socketError;
        }

        private static unsafe SocketError Send(Socket socket, Buffer<byte> buffer)
        {
            buffer.TryGetArray(out var bufferSegment);
            fixed (byte* bufferPtr = &bufferSegment.Array[bufferSegment.Offset])
            {
                var wsaBuffer = new WSABuffer
                {
                    Length = buffer.Length,
                    Pointer = (IntPtr)bufferPtr
                };

                return WSASend(
                    socket.Handle,
                    &wsaBuffer,
                    1,
                    out _,
                    SocketFlags.None,
                    null,
                    IntPtr.Zero);
            }
        }

        [DllImport("ws2_32", SetLastError = true)]
        internal static unsafe extern SocketError WSARecv(
            IntPtr socketHandle,
            WSABuffer* buffers,
            int bufferCount,
            out int bytesTransferred,
            ref SocketFlags socketFlags,
            NativeOverlapped* overlapped,
            IntPtr completionRoutine);

        [DllImport("ws2_32", SetLastError = true)]
        private static extern unsafe SocketError WSASend(
            IntPtr socketHandle,
            WSABuffer* buffers,
            int bufferCount,
            out int bytesTransferred,
            SocketFlags socketFlags,
            NativeOverlapped* overlapped,
            IntPtr completionRoutine);

        [DllImport("ws2_32", SetLastError = true)]
        internal static unsafe extern bool WSAGetOverlappedResult(
            IntPtr socketHandle,
            NativeOverlapped* overlapped,
            out uint bytesTransferred,
            bool wait,
            out SocketFlags socketFlags);

        [DllImport("kernel32.dll")]
        private static unsafe extern bool SetFileCompletionNotificationModes(
            IntPtr handle,
            FileCompletionNotificationModes flags);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSABuffer
        {
            internal int Length; // Length of Buffer
            internal IntPtr Pointer;// Pointer to Buffer
        }

        [Flags]
        internal enum FileCompletionNotificationModes : byte
        {
            None = 0,
            SkipCompletionPortOnSuccess = 1,
            SkipSetEventOnHandle = 2
        }

        private class UnownedSocketHandle : SafeHandle
        {
            public UnownedSocketHandle(Socket socket)
                : base(socket.Handle, ownsHandle: false)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }

        private class SocketAwaitable : ICriticalNotifyCompletion
        {
            private readonly static Action _callbackCompleted = () => { };

            private Action _callback;
            private int _bytesTransfered;
            private SocketError _error;

            public SocketAwaitable GetAwaiter() => this;
            public bool IsCompleted => _callback == _callbackCompleted;

            public int GetResult()
            {
                _callback = null;

                if (_error != SocketError.Success)
                {
                    throw new SocketException((int)_error);
                }

                return _bytesTransfered;
            }

            public void OnCompleted(Action continuation)
            {
                if (_callback == _callbackCompleted ||
                    Interlocked.CompareExchange(ref _callback, continuation, null) == _callbackCompleted)
                {
                    continuation();
                }
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                OnCompleted(continuation);
            }

            public void Complete(int numBytes, SocketError socketError)
            {
                _error = socketError;
                _bytesTransfered = numBytes;
                Interlocked.Exchange(ref _callback, _callbackCompleted)?.Invoke();
            }
        }
    }
}
