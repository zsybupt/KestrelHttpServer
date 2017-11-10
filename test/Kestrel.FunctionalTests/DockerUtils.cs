using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    internal static class DockerUtils
    {
        public static bool HasLocalDocker => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            File.Exists(@"\\.\pipe\docker_engine") :
            File.Exists("/var/run/docker.sock");

        public static IDictionary<string, string> GetTestLabels(string runName)
        {
            return new Dictionary<string, string>()
            {
                { "net.asp.test", "1" },
                { "net.asp.test.run", runName },
            };
        }

        public static DockerClient GetLocalClient(ILoggerFactory loggerFactory)
        {
            var url = GetLocalDockerUrl();
            return new DockerClientConfiguration(url, new LoggingCredentials(loggerFactory.CreateLogger<DockerClient>()))
                .CreateClient();
        }

        private static Uri GetLocalDockerUrl()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new Uri("npipe://./pipe/docker_engine");
            }
            else
            {
                // Even on macOS, they proxy the VM's socket to a local unix socket.
                return new Uri("unix://var/run/docker.sock");
            }
        }

        private class LoggingCredentials : Credentials
        {
            private ILogger<DockerClient> _logger;

            public LoggingCredentials(ILogger<DockerClient> logger)
            {
                _logger = logger;
            }

            public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
            {
                return new LoggingHttpMessageHandler(innerHandler, _logger);
            }

            public override bool IsTlsCredentials()
            {
                return false;
            }
        }

        private class LoggingHttpMessageHandler : DelegatingHandler
        {
            private ILogger<DockerClient> _logger;

            public LoggingHttpMessageHandler(HttpMessageHandler innerHandler, ILogger<DockerClient> logger) : base(innerHandler)
            {
                _logger = logger;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("-> {Method} {Url}", request.Method, request.RequestUri);
                var resp = await base.SendAsync(request, cancellationToken);
                _logger.LogInformation("<- {StatusCode} {Url}", resp.StatusCode, request.RequestUri);
                return resp;
            }
        }
    }
}
