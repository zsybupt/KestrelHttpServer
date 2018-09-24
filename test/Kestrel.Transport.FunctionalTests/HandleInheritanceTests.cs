// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Testing;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public class HandleInheritanceTests : TestApplicationErrorLoggerLoggedTest
    {
        [ConditionalFact]
        public async Task SpawnChildProcess_DoesNotInheritListenHandle()
        {
            var hostBuilder = TransportSelector.GetWebHostBuilder()
                .UseKestrel()
                .ConfigureServices(AddTestLogging)
                .UseUrls("http://127.0.0.1:0")
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello World");
                    });
                });

            using (var host = hostBuilder.Build())
            {
                await host.StartAsync();

                var processInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "vi",
                    CreateNoWindow = true,
                };
                using (var process = Process.Start(processInfo))
                {
                    var port = host.GetPort();
                    await host.StopAsync();

                    await Assert.ThrowsAnyAsync<SocketException>(async () =>
                    {
                        // Mac sockets are flaky
                        for (var i = 0; i < 5; i++)
                        {
                            // We should not be able to connect if the handle was correctly closed and not inherited by the child process.
                            using (var client = new TcpClient())
                            {
                                await client.ConnectAsync("127.0.0.1", port).DefaultTimeout();
                            }
                        }
                    });

                    process.Kill();
                }
            }
        }

        [ConditionalFact]
        public async Task SpawnChildProcess_DoesNotInheritConnectionHandle()
        {
            var hostBuilder = TransportSelector.GetWebHostBuilder()
                .UseKestrel()
                .ConfigureServices(AddTestLogging)
                .UseUrls("http://127.0.0.1:0")
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello World");
                    });
                });

            using (var host = hostBuilder.Build())
            {
                await host.StartAsync();

                using (var client = new TcpClient())
                {
                    // First connect and then spawn a child process
                    await client.ConnectAsync("127.0.0.1", host.GetPort());
                    var stream = client.GetStream();

                    var processInfo = new ProcessStartInfo
                    {
                        FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "vi",
                        CreateNoWindow = true,
                    };
                    using (var process = Process.Start(processInfo))
                    {
                        await host.StopAsync();

                        // The connection should fail when the server shuts down if it wasn't inherited by the child process.
                        try
                        {
                            var read = await stream.ReadAsync(new byte[100], 0, 100).DefaultTimeout();
                            Assert.Equal(0, read);
                        }
                        catch (IOException)
                        {
                        }

                        process.Kill();
                    }
                }
            }
        }
    }
}
