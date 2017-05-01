using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal
{
    public class ConnectionPipelineHandler : IConnectionHandler
    {
        private readonly ListenOptions _listenOptions;
        private readonly ServiceContext _serviceContext;

        public ConnectionPipelineHandler(ListenOptions listenOptions, ServiceContext serviceContext)
        {
            _listenOptions = listenOptions;
            _serviceContext = serviceContext;
        }

        public IConnectionContext OnConnection(IConnectionInformation connectionInfo)
        {
            var inputPipe = connectionInfo.PipeFactory.Create(GetInputPipeOptions(connectionInfo.InputWriterScheduler));
            var outputPipe = connectionInfo.PipeFactory.Create(GetOutputPipeOptions(connectionInfo.OutputReaderScheduler));

            var connectionId = CorrelationIdGenerator.GetNextId();

            var context = new ConnectionContext(_listenOptions.Application, _serviceContext)
            {
                ConnectionId = connectionId,
                Adapters = _listenOptions.ConnectionAdapters,
                ConnectionInfo = connectionInfo,
                Input = inputPipe,
                Output = outputPipe
            };

            _ = context.Start();

            return context;
        }

        // Internal for testing
        internal PipeOptions GetInputPipeOptions(IScheduler writerScheduler) => new PipeOptions
        {
            ReaderScheduler = _serviceContext.ThreadPool,
            WriterScheduler = writerScheduler,
            MaximumSizeHigh = _serviceContext.ServerOptions.Limits.MaxRequestBufferSize ?? 0,
            MaximumSizeLow = _serviceContext.ServerOptions.Limits.MaxRequestBufferSize ?? 0
        };

        internal PipeOptions GetOutputPipeOptions(IScheduler readerScheduler) => new PipeOptions
        {
            ReaderScheduler = readerScheduler,
            WriterScheduler = _serviceContext.ThreadPool,
            MaximumSizeHigh = GetOutputResponseBufferSize(),
            MaximumSizeLow = GetOutputResponseBufferSize()
        };

        private long GetOutputResponseBufferSize()
        {
            var bufferSize = _serviceContext.ServerOptions.Limits.MaxResponseBufferSize;
            if (bufferSize == 0)
            {
                // 0 = no buffering so we need to configure the pipe so the the writer waits on the reader directly
                return 1;
            }

            // null means that we have no back pressure
            return bufferSize ?? 0;
        }

        private class ConnectionContext : IConnectionContext
        {
            private ServiceContext _serviceContext;

            public ConnectionContext(IApplication application, ServiceContext serviceContext)
            {
                Application = application;
                _serviceContext = serviceContext;
            }

            // Internal for testing
            internal PipeOptions AdaptedInputPipeOptions => new PipeOptions
            {
                ReaderScheduler = _serviceContext.ThreadPool,
                WriterScheduler = InlineScheduler.Default,
                MaximumSizeHigh = _serviceContext.ServerOptions.Limits.MaxRequestBufferSize ?? 0,
                MaximumSizeLow = _serviceContext.ServerOptions.Limits.MaxRequestBufferSize ?? 0
            };

            internal PipeOptions AdaptedOutputPipeOptions => new PipeOptions
            {
                ReaderScheduler = InlineScheduler.Default,
                WriterScheduler = InlineScheduler.Default,
                MaximumSizeHigh = _serviceContext.ServerOptions.Limits.MaxResponseBufferSize ?? 0,
                MaximumSizeLow = _serviceContext.ServerOptions.Limits.MaxResponseBufferSize ?? 0
            };

            public string ConnectionId { get; set; }

            public List<IConnectionAdapter> Adapters { get; set; }

            public IApplication Application { get; }

            public IConnectionInformation ConnectionInfo { get; set; }

            public IPipe Input { get; set; }

            public IPipe Output { get; set; }

            IPipeWriter IConnectionContext.Input => Input.Writer;

            IPipeReader IConnectionContext.Output => Output.Reader;

            public void Abort(Exception ex)
            {
            }

            public void OnConnectionClosed(Exception ex)
            {
            }

            public async Task Start()
            {
                _serviceContext.Log.ConnectionStart(ConnectionId);

                if (Adapters.Count == 0)
                {
                    await Application.ExecuteAsync(ConnectionId, ConnectionInfo, Input.Reader, Output.Writer);
                }
                else
                {
                    var stream = new RawStream(Input.Reader, Output.Writer);
                    var adapterContext = new ConnectionAdapterContext(stream);
                    var adaptedConnections = new IAdaptedConnection[Adapters.Count];

                    // TODO: Handle failure here
                    for (var i = 0; i < Adapters.Count; i++)
                    {
                        var adaptedConnection = await Adapters[i].OnConnectionAsync(adapterContext);
                        adaptedConnections[i] = adaptedConnection;
                        adapterContext = new ConnectionAdapterContext(adaptedConnection.ConnectionStream);
                    }

                    // The stream wasn't modified
                    if (adapterContext.ConnectionStream != stream)
                    {
                        using (var filteredStream = adapterContext.ConnectionStream)
                        {
                            var adaptedPipeline = new AdaptedPipeline(
                                adapterContext.ConnectionStream,
                                ConnectionInfo.PipeFactory.Create(AdaptedInputPipeOptions),
                                ConnectionInfo.PipeFactory.Create(AdaptedOutputPipeOptions));

                            var pipelineTask = adaptedPipeline.RunAsync();
                            var applicationTask = Application.ExecuteAsync(ConnectionId, ConnectionInfo, adaptedPipeline.Input.Reader, adaptedPipeline.Output.Writer);

                            await applicationTask;
                            await pipelineTask;
                        }
                    }
                    else
                    {
                        await Application.ExecuteAsync(ConnectionId, ConnectionInfo, Input.Reader, Output.Writer);
                    }
                }
            }
        }
    }

}
