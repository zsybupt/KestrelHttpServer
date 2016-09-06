// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Filter.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Http;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Networking;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.Internal
{
    public class KestrelEngine : ServiceContext, IDisposable
    {
        public KestrelEngine(ServiceContext context)
            : this(new Libuv(), context)
        { }

        // For testing
        internal KestrelEngine(Libuv uv, ServiceContext context)
           : base(context)
        {
            Libuv = uv;
            Threads = new List<KestrelThread>();
        }

        public Libuv Libuv { get; private set; }
        public List<KestrelThread> Threads { get; private set; }

        public void Start(int count)
        {
            for (var index = 0; index < count; index++)
            {
                Threads.Add(new KestrelThread(this));
            }

            foreach (var thread in Threads)
            {
                thread.StartAsync().Wait();
            }
        }

        public void Dispose()
        {
            Task.WaitAll(Threads.Select(thread => thread.StopAsync(TimeSpan.FromSeconds(2.5))).ToArray());

            Threads.Clear();
#if DEBUG
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
#endif
        }

        private class App : IHttpApplication<HttpContext>
        {
            public HttpContext CreateContext(IFeatureCollection contextFeatures)
            {
                return null;
            }

            public Task ProcessRequestAsync(HttpContext context)
            {
                return TaskUtilities.CompletedTask;
            }

            public void DisposeContext(HttpContext context, Exception exception)
            {
            }
        }

        private static readonly byte[] _requestBytes = Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n");

        public IDisposable CreateServer(ServerAddress address)
        {
            var memory = new MemoryPool();
            var threadPool = new LoggingThreadPool(Log);

            var socketInput = new SocketInput(memory, threadPool);
            var memoryStream = new MemoryStream();
            var socketOutput = new StreamSocketOutput("foo", memoryStream, memory, Log);
            var produceBuffer = socketInput.IncomingStart();
            Buffer.BlockCopy(_requestBytes, 0, produceBuffer.Array, produceBuffer.Start, _requestBytes.Length);
            socketInput.IncomingComplete(_requestBytes.Length, null);
            socketInput.IncomingComplete(0, null);

            var context = new ConnectionContext
            {
                ServerAddress = address,
                ServerOptions = new KestrelServerOptions(),
                SocketInput = socketInput,
                SocketOutput = socketOutput,
                Log = Log,
                DateHeaderValueManager = DateHeaderValueManager
            };

            var frame = new Frame<HttpContext>(new App(), context);
            frame.Start();
            Task.Delay(100).Wait();
            frame.Stop().Wait();
            socketInput.Dispose();
            memory.Dispose();

            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            var response = streamReader.ReadToEnd();
            Console.WriteLine(response);
            memoryStream.Dispose();

            var listeners = new List<IAsyncDisposable>();

            var usingPipes = address.IsUnixPipe;

            try
            {
                var pipeName = (Libuv.IsWindows ? @"\\.\pipe\kestrel_" : "/tmp/kestrel_") + Guid.NewGuid().ToString("n");

                var single = Threads.Count == 1;
                var first = true;

                foreach (var thread in Threads)
                {
                    if (single)
                    {
                        var listener = usingPipes ?
                            (Listener) new PipeListener(this) :
                            new TcpListener(this);
                        listeners.Add(listener);
                        listener.StartAsync(address, thread).Wait();
                    }
                    else if (first)
                    {
                        var listener = usingPipes
                            ? (ListenerPrimary) new PipeListenerPrimary(this)
                            : new TcpListenerPrimary(this);

                        listeners.Add(listener);
                        listener.StartAsync(pipeName, address, thread).Wait();
                    }
                    else
                    {
                        var listener = usingPipes
                            ? (ListenerSecondary) new PipeListenerSecondary(this)
                            : new TcpListenerSecondary(this);
                        listeners.Add(listener);
                        listener.StartAsync(pipeName, address, thread).Wait();
                    }

                    first = false;
                }

                return new Disposable(() =>
                {
                    DisposeListeners(listeners);
                });
            }
            catch
            {
                DisposeListeners(listeners);

                throw;
            }
        }

        private void DisposeListeners(List<IAsyncDisposable> listeners)
        {
            var disposeTasks = listeners.Select(listener => listener.DisposeAsync()).ToArray();

            if (!Task.WaitAll(disposeTasks, TimeSpan.FromSeconds(2.5)))
            {
                Log.LogError(0, null, "Disposing listeners failed");
            }
        }
    }
}
