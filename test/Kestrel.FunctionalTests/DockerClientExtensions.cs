using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public static class DockerClientExtensions
    {
        public static async Task<NetworksCreateResponse> CreateTestBridgeNetworkAsync(this INetworkOperations networkOperations, string runName, ILogger logger, CancellationToken cancellationToken)
        {
            var networkName = $"net_{runName}";
            logger.LogInformation("Creating docker network {NetworkName}", networkName);
            var network = await networkOperations.CreateNetworkAsync(new NetworksCreateParameters()
            {
                Name = networkName,
                EnableIPv6 = false,
                Driver = "bridge",
                Labels = DockerUtils.GetTestLabels(runName)
            }, cancellationToken);
            logger.LogInformation("Created docker network {NetworkName}", networkName);
            return network;
        }

        public static async Task<CreateContainerResponse> RunContainerAsync(this IContainerOperations containerOperations, string name, CreateContainerParameters parameters, ILogger logger, CancellationToken cancellationToken)
        {
            using (logger.BeginScope("Running container {ContainerName} using image {ContainerImage}", name, parameters.Image))
            {
                parameters.Name = name;

                logger.LogInformation("Creating container {ContainerName}", parameters.Name);
                var containerResponse = await containerOperations.CreateContainerAsync(parameters, cancellationToken);
                DumpWarnings(containerResponse.Warnings, logger, cancellationToken);
                logger.LogInformation("Created container {ContainerName}", parameters.Name);

                logger.LogInformation("Starting container {ContainerName}", parameters.Name);
                Assert.True(await containerOperations.StartContainerAsync(parameters.Name, new ContainerStartParameters(), cancellationToken));
                logger.LogInformation("Started container {ContainerName}", parameters.Name);

                return containerResponse;
            }
        }

        public static async Task WaitForHealthyAsync(this IContainerOperations containerOperations, string containerName, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("Waiting for container to become healthy...");
            while (true)
            {
                var status = await containerOperations.InspectContainerAsync(containerName);
                cancellationToken.ThrowIfCancellationRequested();
                if (string.Equals(status.State.Health.Status, "healthy"))
                {
                    return;
                }
                await Task.Delay(200);
            }
        }

        public static async Task<ContainerWaitResponse> WaitContainerAsync(this IContainerOperations containerOperations, string containerName, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("Waiting for container {ContainerName} to exit", containerName);
            var response = await containerOperations.WaitContainerAsync(containerName, cancellationToken);
            logger.LogInformation("Container {ContainerName} exited with exit code {ExitCode}", containerName, response.StatusCode);
            return response;
        }

        public static async Task<ContainerWaitResponse> KillContainerAndWaitForExitAsync(this IContainerOperations containerOperations, string containerName, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping {ContainerName}", containerName);
            await containerOperations.KillContainerAsync(containerName, new ContainerKillParameters(), cancellationToken);
            logger.LogInformation("Stop request sent to {ContainerName}", containerName);
            return await containerOperations.WaitContainerAsync(containerName, logger, cancellationToken);
        }

        public static async Task SafeDumpLogsAsync(this IContainerOperations containerOperations, string containerName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Getting logs for container {ContainerName}", containerName);
                using (var logStream = await containerOperations.GetContainerLogsAsync(containerName, new ContainerLogsParameters() { ShowStderr = true, ShowStdout = true }))
                {
                    await DumpMessageStreamAsync(containerName, logStream, logger, cancellationToken);
                }
                logger.LogInformation("End of logs for container {ContainerName}", containerName);
            }
            catch (Exception ex)
            {
                if (ex is DockerApiException apiex)
                {
                    logger.LogError(ex, "Error dumping logs for '{ContainerName}': {StatusCode} {Message}", apiex.StatusCode, apiex.Message, containerName);
                }
                else
                {
                    logger.LogError(ex, "{ExceptionType} dumping logs for {ContainerName}", ex.GetType().FullName, containerName);
                }
            }
        }

        public static async Task SafeRemoveNetworkAsync(this INetworkOperations networkOperations, string networkName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Removing network {NetworkName}", networkName);
                await networkOperations.DeleteNetworkAsync(networkName, cancellationToken);
                logger.LogInformation("Removed network {NetworkName}", networkName);
            }
            catch (Exception ex)
            {
                if (ex is DockerApiException apiex)
                {
                    logger.LogError(ex, "Error removing network '{NetworkName}': {StatusCode} {Message}", apiex.StatusCode, apiex.Message, networkName);
                }
                else
                {
                    logger.LogError(ex, "{ExceptionType} removing network: {NetworkName}", ex.GetType().FullName, networkName);
                }
            }
        }

        public static async Task SafeRemoveContainerAsync(this IContainerOperations containerOperations, string containerName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Removing container {ContainerName}", containerName);
                await containerOperations.RemoveContainerAsync(containerName, new ContainerRemoveParameters()
                {
                    Force = true
                }, cancellationToken);
                logger.LogInformation("Removed container {ContainerName}", containerName);
            }
            catch (Exception ex)
            {
                if (ex is DockerApiException apiex)
                {
                    logger.LogTrace(ex, "Error removing container '{ContainerName}': {StatusCode} {Message}", apiex.StatusCode, apiex.Message, containerName);
                }
                else
                {
                    logger.LogTrace(ex, "{ExceptionType} removing container: {ContainerName}", ex.GetType().FullName, containerName);
                }
            }
        }

        // Useful builder methods
        public static CreateContainerParameters WithImage(this CreateContainerParameters self, string image) => With(self, s => s.Image = image);
        public static CreateContainerParameters WithCmd(this CreateContainerParameters self, params string[] cmd) => With(self, s => s.Cmd = cmd.ToList());
        public static CreateContainerParameters WithBinds(this CreateContainerParameters self, params string[] binds) => With(self, s => s.GetHostConfig().Binds = binds.ToList());
        public static CreateContainerParameters WithNetwork(this CreateContainerParameters self, string network) => self.WithNetwork(network, new EndpointSettings());
        public static CreateContainerParameters WithNetwork(this CreateContainerParameters self, string network, EndpointSettings settings) => With(self, s => s.GetNetworkingConfig().GetEndpointConfig().Add(network, settings));
        public static CreateContainerParameters WithHealthcheck(this CreateContainerParameters self, Action<HealthConfig> configurer)
        {
            return With(self, s =>
            {
                var config = new HealthConfig();
                configurer(config);
                s.Healthcheck = config;
            });
        }

        public static CreateContainerParameters WithHttpHealthcheck(this CreateContainerParameters self, string url)
        {
            return self.WithHealthcheck(config =>
            {
                config.Interval = TimeSpan.FromSeconds(1);
                config.Retries = 0;
                config.Test = new List<string>() { "CMD", "curl", "-k", url };
            });
        }

        private static T With<T>(T target, Action<T> act)
        {
            act(target);
            return target;
        }

        private static HostConfig GetHostConfig(this CreateContainerParameters self)
        {
            if (self.HostConfig == null)
            {
                self.HostConfig = new HostConfig();
            }
            return self.HostConfig;
        }

        private static NetworkingConfig GetNetworkingConfig(this CreateContainerParameters self)
        {
            if (self.NetworkingConfig == null)
            {
                self.NetworkingConfig = new NetworkingConfig();
            }
            return self.NetworkingConfig;
        }

        private static IDictionary<string, EndpointSettings> GetEndpointConfig(this NetworkingConfig self)
        {
            if (self.EndpointsConfig == null)
            {
                self.EndpointsConfig = new Dictionary<string, EndpointSettings>();
            }
            return self.EndpointsConfig;
        }

        private static void DumpWarnings(IList<string> warnings, ILogger logger, CancellationToken cancellationToken)
        {
            if (warnings != null)
            {
                foreach (var warning in warnings)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    logger.LogWarning(FilterAnsiEscapes(warning));
                }
            }
        }

        private static readonly Regex _ansiColorEscapeRegex = new Regex("\x1b[^m]*m");
        private static string FilterAnsiEscapes(string input)
        {
            return _ansiColorEscapeRegex.Replace(input, "");
        }

        private static async Task DumpMessageStreamAsync(string container, Stream logStream, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            var headerBuf = new byte[8];
            var payloadBuf = new byte[128];
            while (await TryReadExactAsync(logStream, headerBuf, 0, 8) == 8)
            {
                var length =
                    headerBuf[4] << 24 |
                    headerBuf[5] << 16 |
                    headerBuf[6] << 8 |
                    headerBuf[7];

                if (length > payloadBuf.Length)
                {
                    payloadBuf = new byte[length];
                }

                // It's OK if the stream ends mid-read here, we'll catch it again in the while
                var actualRead = await TryReadExactAsync(logStream, payloadBuf, 0, length);

                var payload = FilterAnsiEscapes(Encoding.UTF8.GetString(payloadBuf, 0, actualRead));
                using (var reader = new StringReader(payload))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (headerBuf[0] == 2)
                        {
                            logger.LogError("{ContainerName}: {Message}", container, line);
                        }
                        else
                        {
                            logger.LogTrace("{ContainerName}: {Message}", container, line);
                        }
                    }
                }
            }
        }

        private static async Task<int> TryReadExactAsync(Stream stream, byte[] array, int offset, int count)
        {
            var readSoFar = 0;
            while (readSoFar < count)
            {
                var readThisRound = await stream.ReadAsync(array, readSoFar, count - readSoFar);
                if (readThisRound == 0)
                {
                    return readSoFar;
                }
                readSoFar += readThisRound;
            }
            return readSoFar;
        }
    }
}
