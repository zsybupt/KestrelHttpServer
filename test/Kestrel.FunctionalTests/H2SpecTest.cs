using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public class H2SpecTest : LoggedTest
    {
        public H2SpecTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task RunH2SpecTests()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            using (StartLog(out var loggerFactory))
            {
                var logger = loggerFactory.CreateLogger<H2SpecTest>();

                var docker = DockerUtils.GetLocalClient(loggerFactory);

                // Generate a unique "name" for the run
                var runName = $"Kestrel_{nameof(H2SpecTest)}_{DateTime.Now:yyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")}";
                var networkName = $"net_{runName}";
                var serverContainerName = $"kestrel_server"; // _{runName}";
                var specContainerName = $"kestrel_spec"; // _{runName}";
                var labels = new Dictionary<string, string>()
                {
                    {  "net.asp.test.run", runName }
                };

                // Remove the container if it's already here for some reason
                await SafeRemoveContainerAsync(docker, serverContainerName, logger, cts.Token);
                await SafeRemoveContainerAsync(docker, specContainerName, logger, cts.Token);

                try
                {

                    // Create a network for the test
                    logger.LogInformation("Creating docker network {NetworkName}", runName);
                    var network = await docker.Networks.CreateNetworkAsync(new NetworksCreateParameters()
                    {
                        Name = networkName,
                        EnableIPv6 = false,
                        Driver = "bridge",
                        Labels = labels
                    }, cts.Token);
                    logger.LogInformation("Created docker network {NetworkName}", runName);

                    // Start the container using 'tail -f /dev/null' so we can exec into it
                    // Also, host bind-mount the project directory.
                    var projectRoot = @"C:\Users\anurse\Code\aspnet\KestrelHttpServer\samples\Http2SampleApp\bin\Debug\netcoreapp2.0\publish";
                    using (logger.BeginScope("Creating container: {ContainerName}", serverContainerName))
                    {
                        logger.LogInformation("Creating container {ContainerName}", serverContainerName);
                        var containerResponse = await docker.Containers.CreateContainerAsync(new CreateContainerParameters()
                        {
                            Image = "microsoft/aspnetcore:2.0",
                            Name = serverContainerName,
                            Cmd = new List<string>() { "/app/scripts/container-start.sh" },
                            Tty = false,
                            HostConfig = new HostConfig()
                            {
                                Binds = new List<string>() { $"{projectRoot}:/app" }
                            },
                            NetworkingConfig = new NetworkingConfig()
                            {
                                EndpointsConfig = new Dictionary<string, EndpointSettings>()
                                {
                                    { networkName, new EndpointSettings() }
                                }
                            },
                            Healthcheck = new HealthConfig()
                            {
                                Test = new List<string>() { "CMD", "curl", "-k", "https://localhost:5000" },
                                Interval = TimeSpan.FromSeconds(1),
                                Retries = 0
                            }
                        }, cts.Token);
                        DumpWarnings(containerResponse.Warnings, logger, cts.Token);
                        logger.LogInformation("Created container {ContainerName}", serverContainerName);

                        logger.LogInformation("Starting container {ContainerName}", serverContainerName);
                        Assert.True(await docker.Containers.StartContainerAsync(serverContainerName, new ContainerStartParameters(), cts.Token));
                        logger.LogInformation("Started container {ContainerName}", serverContainerName);
                    }

                    // Wait for the container to become healthy
                    await WaitForHealthyAsync(docker, serverContainerName, logger, cts.Token);
                    logger.LogInformation("Server container started");

                    // Run the h2spec container
                    using (logger.BeginScope("Running h2spec container: {ContainerName}", specContainerName))
                    {
                        logger.LogInformation("Creating container {ContainerName}", specContainerName);
                        var containerResponse = await docker.Containers.CreateContainerAsync(new CreateContainerParameters()
                        {
                            Image = "summerwind/h2spec",
                            Name = specContainerName,
                            Cmd = new List<string>() { "--host", serverContainerName, "--port", "5000", "-k", "-t" },
                            Tty = false,
                            NetworkingConfig = new NetworkingConfig()
                            {
                                EndpointsConfig = new Dictionary<string, EndpointSettings>()
                                {
                                    { networkName, new EndpointSettings() }
                                }
                            },
                        }, cts.Token);
                        DumpWarnings(containerResponse.Warnings, logger, cts.Token);
                        logger.LogInformation("Created container {ContainerName}", specContainerName);

                        logger.LogInformation("Starting container {ContainerName}", specContainerName);
                        Assert.True(await docker.Containers.StartContainerAsync(specContainerName, new ContainerStartParameters(), cts.Token));
                        logger.LogInformation("Started container {ContainerName}", specContainerName);
                    }

                    // Wait for h2spec to stop
                    logger.LogInformation("Waiting for container {ContainerName} to exit", specContainerName);
                    var response = await docker.Containers.WaitContainerAsync(specContainerName, cts.Token);
                    logger.LogInformation("Container {ContainerName} exited with exit code {ExitCode}", specContainerName, response.StatusCode);
                    Assert.Equal(0, response.StatusCode);

                    // Stop and wait for server to stop
                    logger.LogInformation("Stopping {ContainerName}", serverContainerName);
                    Assert.True(await docker.Containers.StopContainerAsync(serverContainerName, new ContainerStopParameters(), cts.Token));
                    logger.LogInformation("Stop request sent to {ContainerName}", serverContainerName);
                    response = await docker.Containers.WaitContainerAsync(serverContainerName, cts.Token);
                    logger.LogInformation("Container {ContainerName} exited with exit code: {ExitCode}", serverContainerName, response.StatusCode);
                    Assert.Equal(0, response.StatusCode);
                }
                finally
                {
                    // Dump logs and clean up containers
                    await DumpLogsAsync(docker, serverContainerName, logger, cts.Token);
                    await DumpLogsAsync(docker, specContainerName, logger, cts.Token);
                    await SafeRemoveContainerAsync(docker, serverContainerName, logger, cts.Token);
                    await SafeRemoveContainerAsync(docker, specContainerName, logger, cts.Token);
                    await SafeRemoveNetworkAsync(docker, networkName, logger, cts.Token);
                }
            }
        }

        private async Task SafeRemoveNetworkAsync(DockerClient docker, string networkName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Removing network {NetworkName}", networkName);
                await docker.Networks.DeleteNetworkAsync(networkName, cancellationToken);
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

        private async Task SafeRemoveContainerAsync(DockerClient docker, string containerName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Removing container {ContainerName}", containerName);
                await docker.Containers.RemoveContainerAsync(containerName, new ContainerRemoveParameters()
                {
                    Force = true
                }, cancellationToken);
                logger.LogInformation("Removed container {ContainerName}", containerName);
            }
            catch (Exception ex)
            {
                if (ex is DockerApiException apiex)
                {
                    logger.LogError(ex, "Error removing container '{ContainerName}': {StatusCode} {Message}", apiex.StatusCode, apiex.Message, containerName);
                }
                else
                {
                    logger.LogError(ex, "{ExceptionType} removing container: {ContainerName}", ex.GetType().FullName, containerName);
                }
            }
        }

        private async Task DumpLogsAsync(DockerClient docker, string containerName, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Getting logs for container {ContainerName}", containerName);
                using (var logStream = await docker.Containers.GetContainerLogsAsync(containerName, new ContainerLogsParameters() { ShowStderr = true, ShowStdout = true }))
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

        private async Task DumpMessageStreamAsync(string container, Stream logStream, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
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
                var lines = payload.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                for (var i = 0; i < lines.Length; i += 1)
                {
                    // If the last line is empty, skip it. But only for the last line, to not break formatting.
                    if(string.IsNullOrEmpty(lines[i]) || i == lines.Length - 1)
                    {
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    if (headerBuf[0] == 2)
                    {
                        logger.LogError("{ContainerName}: {Message}", container, lines[i]);
                    }
                    else
                    {
                        logger.LogInformation("{ContainerName}: {Message}", container, lines[i]);
                    }
                }
            }
        }

        private async Task WaitForHealthyAsync(DockerClient docker, string serverContainerName, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("Waiting for container to become healthy...");
            while (true)
            {
                var status = await docker.Containers.InspectContainerAsync(serverContainerName);
                cancellationToken.ThrowIfCancellationRequested();
                if (string.Equals(status.State.Health.Status, "healthy"))
                {
                    return;
                }
                await Task.Delay(200);
            }
        }

        private void DumpWarnings(IList<string> warnings, ILogger<H2SpecTest> logger, CancellationToken cancellationToken)
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
