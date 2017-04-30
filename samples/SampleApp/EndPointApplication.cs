using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;
using Microsoft.AspNetCore.Sockets;
using Microsoft.AspNetCore.Sockets.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace SampleApp
{
    public class MyEndPoint : EndPoint
    {
        public override async Task OnConnectedAsync(Connection connection)
        {
            while (true)
            {
                // Echo server
                var message = await connection.Transport.Input.ReadAsync();
                await connection.Transport.Output.WriteAsync(message);
            }
        }
    }

    public static class ListenOptionsExtensions
    {
        public static ListenOptions UseEndPoint<TEndpoint>(this ListenOptions options) where TEndpoint : EndPoint
        {
            var endpoint = options.KestrelServerOptions.ApplicationServices.GetRequiredService<TEndpoint>();
            var manager = options.KestrelServerOptions.ApplicationServices.GetRequiredService<ConnectionManager>();
            options.ConnectionAdapters.Add(new EndPointConnectionAdapter(endpoint, manager));
            return options;
        }
    }

    public class EndPointConnectionAdapter : IConnectionAdapter
    {
        public bool IsHttps => false;

        private readonly EndPoint _endPoint;
        private readonly ConnectionManager _connectionManager;

        public EndPointConnectionAdapter(EndPoint endpoint, ConnectionManager connectionManager)
        {
            _endPoint = endpoint;
            _connectionManager = connectionManager;
        }

        public Task<IAdaptedConnection> OnConnectionAsync(ConnectionAdapterContext context)
        {
            var state = _connectionManager.CreateConnection();

            var stream = new EndpointStream();
            _ = ExecuteConnection(context, state, stream);

            var connection = new EndpointAdaptedConnection(stream);

            return Task.FromResult<IAdaptedConnection>(connection);
        }

        private async Task ExecuteConnection(ConnectionAdapterContext context, ConnectionState state, Stream stream)
        {
            state.Status = ConnectionState.ConnectionStatus.Active;

            state.ApplicationTask = _endPoint.OnConnectedAsync(state.Connection);
            state.TransportTask = ExecuteAsync(context.ConnectionStream, state.Application);

            await Task.WhenAny(state.TransportTask, state.ApplicationTask);

            await _connectionManager.DisposeAndRemoveAsync(state);

            stream.Dispose();
        }

        private async Task ExecuteAsync(Stream connectionStream, IChannelConnection<Message> application)
        {
            var reads = DoReads(connectionStream, application);
            var writes = DoWrites(connectionStream, application);

            await reads;
            await writes;
        }

        private async Task DoWrites(Stream connectionStream, IChannelConnection<Message> application)
        {
            while (await application.Input.WaitToReadAsync())
            {
                while (application.Input.TryRead(out var message))
                {
                    await connectionStream.WriteAsync(message.Payload, 0, message.Payload.Length);
                }
            }
        }

        private static async Task DoReads(Stream connectionStream, IChannelConnection<Message> application)
        {
            var buffer = new byte[4096];
            while (true)
            {
                var read = await connectionStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    break;
                }
                var segment = new ArraySegment<byte>(buffer, 0, read);
                await application.Output.WriteAsync(new Message(segment.ToArray(), MessageType.Text));
            }
        }

        private class EndpointAdaptedConnection : IAdaptedConnection
        {
            public EndpointAdaptedConnection(EndpointStream stream)
            {
                ConnectionStream = stream;
            }

            public Stream ConnectionStream { get; }

            public void PrepareRequest(IFeatureCollection requestFeatures)
            {
            }
        }

        private class EndpointStream : Stream
        {
            private readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _tcs.Task;
            }

            protected override void Dispose(bool disposing)
            {
                _tcs.TrySetResult(0);
                base.Dispose(disposing);
            }
        }
    }

    public class EndpointApplication
    {
        private readonly EndPoint _endpoint;
        private readonly ConnectionManager _manager;

        public EndpointApplication(EndPoint endpoint, ConnectionManager manager)
        {
            _endpoint = endpoint;
            _manager = manager;
        }

        public IPipeReader Input { get; set; }

        public IPipeWriter Output { get; set; }

        public IChannelConnection<Message> Application { get; set; }

        public async Task ExecuteAsync(string connectionId, IConnectionInformation info, IPipeReader reader, IPipeWriter writer)
        {
            Input = reader;
            Output = writer;

            var state = _manager.CreateConnection();
            Application = state.Application;

            state.Status = ConnectionState.ConnectionStatus.Active;

            state.ApplicationTask = _endpoint.OnConnectedAsync(state.Connection);
            state.TransportTask = ExecuteAsync();

            await Task.WhenAny(state.TransportTask, state.ApplicationTask);

            await _manager.DisposeAndRemoveAsync(state);
        }

        public async Task ExecuteAsync()
        {
            var reads = DoReads();
            var writes = DoWrites();

            await reads;
            await writes;
        }

        public async Task DoWrites()
        {
            while (await Application.Input.WaitToReadAsync())
            {
                while (Application.Input.TryRead(out var message))
                {
                    var buffer = Output.Alloc();
                    buffer.Write(message.Payload);
                    await buffer.FlushAsync();
                }
            }

            Output.Complete();
        }

        private async Task DoReads()
        {
            try
            {
                while (true)
                {
                    var result = await Input.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (buffer.IsEmpty && result.IsCompleted)
                        {
                            break;
                        }

                        await Application.Output.WriteAsync(new Message(buffer.ToArray(), MessageType.Text));
                    }
                    finally
                    {
                        Input.Advance(buffer.End);
                    }
                }

                Input.Complete();
            }
            catch (Exception ex)
            {
                Input.Complete(ex);
            }
        }
    }
}
