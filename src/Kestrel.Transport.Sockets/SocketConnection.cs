// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        private const int NumIoThreads = 4;

        private static readonly IntPtr[] _completionPorts = new IntPtr[NumIoThreads];
        private static int _nextIoThread;

        private readonly Socket _socket;
        private readonly ISocketsTrace _trace;

        private readonly SocketAwaitable _receiveAwaitable = new SocketAwaitable();
        private readonly SocketAwaitable _sendAwaitable = new SocketAwaitable();

        private IntPtr _completionPort;
        private IntPtr _packedReceiveOverlapped;
        private IntPtr _packedSendOverlapped;

        private bool _skipCompletionPortOnSuccess;
        private volatile bool _aborted;

        static SocketConnection()
        {
            for (var i = 0; i < NumIoThreads; i++)
            {
                _completionPorts[i] = CreateIoCompletionPort((IntPtr)(-1), IntPtr.Zero, 0, 0);

                var thread = new Thread(ThreadStart);
                thread.IsBackground = true;
                thread.Name = nameof(SocketConnection);
                thread.Start(_completionPorts[i]);
            }
        }

        internal unsafe SocketConnection(Socket socket, PipeFactory pipeFactory, ISocketsTrace trace)
        {
            Debug.Assert(socket != null);
            Debug.Assert(pipeFactory != null);
            Debug.Assert(trace != null);

            _socket = socket;
            _completionPort = _completionPorts[Interlocked.Increment(ref _nextIoThread) % NumIoThreads];

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
            AllocateNativeOverlapped();

            try
            {
                connectionHandler.OnConnection(this);

                CreateIoCompletionPort(_socket.Handle, _completionPort, (uint)_socket.Handle, 0);

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
                FreeNativeOverlapped();
            }
            catch (Exception ex)
            {
                _trace.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}.");
            }
            finally
            {
                FreeNativeOverlapped();
            }
        }

        private unsafe void AllocateNativeOverlapped()
        {
            var receiveOverlapped = new Overlapped
            {
                AsyncResult = new CallbackAsyncResult
                {
                    Callback = (bytesTransferred, nativeOverlappped, state) => ((SocketConnection)state).ReceiveCompletionCallback(bytesTransferred, nativeOverlappped),
                    AsyncState = this
                }
            };
            var sendOverlapped = new Overlapped
            {
                AsyncResult = new CallbackAsyncResult
                {
                    Callback = (bytesTransferred, nativeOverlappped, state) => ((SocketConnection)state).SendCompletionCallback(bytesTransferred, nativeOverlappped),
                    AsyncState = this
                }
            };

            _packedReceiveOverlapped = (IntPtr)receiveOverlapped.UnsafePack(null, null);
            _packedSendOverlapped = (IntPtr)sendOverlapped.UnsafePack(null, null);
        }

        private unsafe void FreeNativeOverlapped()
        {
            Overlapped.Free((NativeOverlapped*)_packedReceiveOverlapped);
            Overlapped.Free((NativeOverlapped*)_packedSendOverlapped);
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
                    (NativeOverlapped*)_packedReceiveOverlapped,
                    IntPtr.Zero);

                errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

                if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
                {
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

            var errno = WSASend(
                _socket.Handle,
                wsaBuffers,
                bufferCount,
                out var bytesTransferred,
                SocketFlags.None,
                (NativeOverlapped*)_packedSendOverlapped,
                IntPtr.Zero);

            errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

            if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
            {
                _sendAwaitable.Complete(bytesTransferred, errno);
            }

            return _sendAwaitable;
        }

        private unsafe SocketAwaitable SendAsync(Buffer<byte> buffer)
        {
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
                    (NativeOverlapped*)_packedSendOverlapped,
                    IntPtr.Zero);

                errno = errno == SocketError.Success ? SocketError.Success : (SocketError)Marshal.GetLastWin32Error();

                if (errno != SocketError.IOPending && (_skipCompletionPortOnSuccess || errno != SocketError.Success))
                {
                    _sendAwaitable.Complete(bytesTransferred, errno);
                }
            }

            return _sendAwaitable;
        }

        private unsafe void ReceiveCompletionCallback( uint bytesTransferred, IntPtr nativeOverlapped)
        {
            _receiveAwaitable.Complete((int)bytesTransferred, GetSocketError((NativeOverlapped*)nativeOverlapped));
        }

        private unsafe void SendCompletionCallback(uint bytesTransferred, IntPtr nativeOverlapped)
        {
            _sendAwaitable.Complete((int)bytesTransferred, GetSocketError((NativeOverlapped*)nativeOverlapped));
        }

        private unsafe SocketError GetSocketError(NativeOverlapped* nativeOverlapped)
        {
            if (_aborted)
            {
                return SocketError.OperationAborted;
            }

            bool success = WSAGetOverlappedResult(
                _socket.Handle,
                nativeOverlapped,
                out _,
                false,
                out _);

            if (success)
            {
                return SocketError.Success;
            }
            else
            {
                return (SocketError)Marshal.GetLastWin32Error();
            }
        }

        private static unsafe void ThreadStart(object parameter)
        {
            var completionPort = (IntPtr)parameter;

            while (true)
            {
                uint bytesTransferred;
                NativeOverlapped* nativeOverlapped;

                var result = GetQueuedCompletionStatus(
                    completionPort,
                    out bytesTransferred,
                    out _,
                    &nativeOverlapped,
                    uint.MaxValue);

                var overlapped = Overlapped.Unpack(nativeOverlapped);

                if (result)
                {
                    var asyncResult = (CallbackAsyncResult)overlapped.AsyncResult;
                    asyncResult.Callback(bytesTransferred, (IntPtr)nativeOverlapped, asyncResult.AsyncState);
                }
                else
                {
                    Trace.Assert(false, $"Unexpectedly failed. errorCode: '{Marshal.GetLastWin32Error()}'");
                }
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
        private static extern bool SetFileCompletionNotificationModes(
            IntPtr handle,
            FileCompletionNotificationModes flags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateIoCompletionPort(
            IntPtr fileHandle,
            IntPtr existingCompletionPort,
            uint completionKey,
            uint numberOfConcurrentThreads);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static unsafe extern bool GetQueuedCompletionStatus(
            IntPtr CompletionPort,
            out uint lpNumberOfBytes,
            out UIntPtr lpCompletionKey,
            NativeOverlapped** lpOverlapped,
            uint dwMilliseconds);

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

        private class CallbackAsyncResult : IAsyncResult
        {
            public Action<uint, IntPtr, object> Callback { get; set; }

            public object AsyncState { get; set; }

            public WaitHandle AsyncWaitHandle => throw new NotImplementedException();

            public bool CompletedSynchronously => throw new NotImplementedException();

            public bool IsCompleted => throw new NotImplementedException();
        }
    }
}
