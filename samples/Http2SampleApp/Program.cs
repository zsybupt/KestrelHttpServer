using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Http2SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            if (!ushort.TryParse(configuration["BASE_PORT"], NumberStyles.None, CultureInfo.InvariantCulture, out var basePort))
            {
                basePort = 5000;
            }

            var hostBuilder = new WebHostBuilder()
                .ConfigureLogging((_, factory) =>
                {
                    // Set logging to the MAX.
                    factory.SetMinimumLevel(LogLevel.Trace);
                    factory.AddConsole();
                })
                .UseKestrel(options =>
                {
                    // Run callbacks on the transport thread
                    options.ApplicationSchedulingMode = SchedulingMode.Inline;

                    options.Listen(IPAddress.Any, basePort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        listenOptions.UseTls(Path.Combine(AppContext.BaseDirectory, "testCert.pfx"), "testPassword");
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

            hostBuilder.Build().Run();
        }
    }
}
