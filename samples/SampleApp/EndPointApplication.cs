using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;
using Microsoft.AspNetCore.SignalR;
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
            options.Application = new EndpointApplication(endpoint, manager);
            return options;
        }

        public static ListenOptions UseHub<THub>(this ListenOptions options) where THub : Hub
        {
            return options.UseEndPoint<HubEndPoint<THub>>();
        }
    }

    public class EndpointApplication : IApplication
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
                    if (message.Payload.Length > 0)
                    {
                        var buffer = Output.Alloc();
                        buffer.Write(message.Payload);
                        await buffer.FlushAsync();
                    }
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

                        // https://github.com/aspnet/SignalR/blob/dev/specs/TransportProtocols.md#binary-encoding-supportsbinary--true

                        if (!buffer.IsEmpty)
                        {
                            await Application.Output.WriteAsync(new Message(buffer.ToArray(), MessageType.Text));
                        }
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
