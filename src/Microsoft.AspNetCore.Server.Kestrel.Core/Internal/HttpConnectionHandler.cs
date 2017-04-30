// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
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

            var context = new ConnectionContext(_serviceContext)
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

            public ConnectionContext(ServiceContext serviceContext)
            {
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

                using (var stream = new RawStream(Input.Reader, Output.Writer))
                {
                    var adapterContext = new ConnectionAdapterContext(stream);
                    var adaptedConnections = new IAdaptedConnection[Adapters.Count];

                    // TODO: Handle failure here
                    for (var i = 0; i < Adapters.Count; i++)
                    {
                        var adaptedConnection = await Adapters[i].OnConnectionAsync(adapterContext);
                        adaptedConnections[i] = adaptedConnection;
                        adapterContext = new ConnectionAdapterContext(adaptedConnection.ConnectionStream);
                    }

                    using (var filteredStream = adapterContext.ConnectionStream)
                    {
                        var adaptedPipeline = new AdaptedPipeline(
                            adapterContext.ConnectionStream,
                            ConnectionInfo.PipeFactory.Create(AdaptedInputPipeOptions),
                            ConnectionInfo.PipeFactory.Create(AdaptedOutputPipeOptions));

                        await adaptedPipeline.RunAsync();
                    }
                }
            }
        }

        public interface IApplication
        {
            void Abort(Exception ex = null);
            void OnConnectionClosed(Exception ex = null);
            Task ExecuteAsync(string connectionId, IConnectionInformation info, IPipeReader reader, IPipeWriter writer);
        }

        public class HttpApplication<TContext> : IApplication
        {
            private readonly IHttpApplication<TContext> _application;
            private readonly ServiceContext _serviceContext;
            private Frame _frame;

            public HttpApplication(ServiceContext serviceContext, IHttpApplication<TContext> application)
            {
                _serviceContext = serviceContext;
                _application = application;
            }

            public void Abort(Exception ex = null)
            {
                _frame.Abort(ex);
            }

            public Task ExecuteAsync(string connectionId, IConnectionInformation info, IPipeReader reader, IPipeWriter writer)
            {
                _frame = new Frame<TContext>(_application, new FrameContext
                {
                    ConnectionId = connectionId,
                    ConnectionInformation = info,
                    ServiceContext = _serviceContext
                });

                return _frame.RequestProcessingAsync();
            }

            public async void OnConnectionClosed(Exception ex = null)
            {
                _frame.Abort(ex);

                await _frame.StopAsync();
            }
        }
    }

    public class HttpConnectionHandler<TContext> : IConnectionHandler
    {
        private static long _lastFrameConnectionId = long.MinValue;

        private readonly ListenOptions _listenOptions;
        private readonly ServiceContext _serviceContext;
        private readonly IHttpApplication<TContext> _application;

        public HttpConnectionHandler(ListenOptions listenOptions, ServiceContext serviceContext, IHttpApplication<TContext> application)
        {
            _listenOptions = listenOptions;
            _serviceContext = serviceContext;
            _application = application;
        }

        public IConnectionContext OnConnection(IConnectionInformation connectionInfo)
        {
            var inputPipe = connectionInfo.PipeFactory.Create(GetInputPipeOptions(connectionInfo.InputWriterScheduler));
            var outputPipe = connectionInfo.PipeFactory.Create(GetOutputPipeOptions(connectionInfo.OutputReaderScheduler));

            var connectionId = CorrelationIdGenerator.GetNextId();
            var frameConnectionId = Interlocked.Increment(ref _lastFrameConnectionId);

            var frameContext = new FrameContext
            {
                ConnectionId = connectionId,
                ConnectionInformation = connectionInfo,
                ServiceContext = _serviceContext
            };

            // TODO: Untangle this mess
            var frame = new Frame<TContext>(_application, frameContext);
            var outputProducer = new OutputProducer(outputPipe.Writer, frame, connectionId, _serviceContext.Log);
            frame.LifetimeControl = new ConnectionLifetimeControl(connectionId, outputPipe.Reader, outputProducer, _serviceContext.Log);

            var connection = new FrameConnection(new FrameConnectionContext
            {
                ConnectionId = connectionId,
                FrameConnectionId = frameConnectionId,
                ServiceContext = _serviceContext,
                PipeFactory = connectionInfo.PipeFactory,
                ConnectionAdapters = _listenOptions.ConnectionAdapters,
                Frame = frame,
                Input = inputPipe,
                Output = outputPipe,
                OutputProducer = outputProducer
            });

            _serviceContext.Log.ConnectionStart(connectionId);
            KestrelEventSource.Log.ConnectionStart(connection, connectionInfo);

            // Since data cannot be added to the inputPipe by the transport until OnConnection returns,
            // Frame.RequestProcessingAsync is guaranteed to unblock the transport thread before calling
            // application code.
            connection.StartRequestProcessing();

            return connection;
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
    }
}

