using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Testing.xunit;
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

        [ConditionalFact(Skip = "Not currently passing, due to incomplete implementation. Tracking issue: https://github.com/aspnet/KestrelHttpServer/issues/2154")]
        [LocalDockerAvailableSkipCondition]
        public async Task RunH2SpecTests()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            NetworksCreateResponse network = null;
            using (StartLog(out var loggerFactory))
            {
                var logger = loggerFactory.CreateLogger<H2SpecTest>();

                var docker = DockerUtils.GetLocalClient(loggerFactory);

                // Generate a unique "name" for the run
                var runName = $"Kestrel_{nameof(H2SpecTest)}_{DateTime.Now:yyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")}";
                var serverContainerName = $"kestrel_server"; // _{runName}";
                var specContainerName = $"kestrel_spec"; // _{runName}";

                // Remove the container if it's already here for some reason
                await docker.Containers.SafeRemoveContainerAsync(serverContainerName, logger, cts.Token);
                await docker.Containers.SafeRemoveContainerAsync(specContainerName, logger, cts.Token);

                // Publish the project
                var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"Kestrel_{nameof(H2SpecTest)}_{Guid.NewGuid().ToString("N")}"));
                var outputDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "output"));
                var projectDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "project"));

                // Publish the project
                var exitCode = await ProjectBuilder.PublishProjectAsync("samples/Http2SampleApp/Http2SampleApp.csproj", "netcoreapp2.0", projectDir.FullName, logger, cts.Token);
                Assert.Equal(0, exitCode);

                try
                {
                    // Create a network for the test
                    network = await docker.Networks.CreateTestBridgeNetworkAsync(runName, logger, cts.Token);

                    // Start the container using 'tail -f /dev/null' so we can exec into it
                    // Also, host bind-mount the project directory.
                    await docker.Containers.RunContainerAsync(serverContainerName, new CreateContainerParameters()
                        .WithImage("microsoft/aspnetcore:2.0")
                        .WithCmd("/app/scripts/container-start.sh")
                        .WithBinds($"{projectDir.FullName}:/app")
                        .WithNetwork(network.ID)
                        .WithHttpHealthcheck("https://localhost:5000"), logger, cts.Token);

                    // Wait for the container to become healthy
                    await docker.Containers.WaitForHealthyAsync(serverContainerName, logger, cts.Token);
                    logger.LogInformation("Server container started");

                    // Run the h2spec container
                    await docker.Containers.RunContainerAsync(specContainerName, new CreateContainerParameters()
                        .WithImage("summerwind/h2spec")
                        .WithCmd("--host", serverContainerName, "--port", "5000", "-k", "-t", "-j", "/tmp/kestrel_tests/h2spec.junit.xml")
                        .WithBinds($"{outputDir.FullName}:/tmp/kestrel_tests")
                        .WithNetwork(network.ID), logger, cts.Token);

                    // Wait for h2spec to stop
                    await docker.Containers.WaitContainerAsync(specContainerName, logger, cts.Token);

                    // Stop the server
                    await docker.Containers.KillContainerAndWaitForExitAsync(serverContainerName, logger, cts.Token);

                    var results = H2SpecResult.LoadResults(Path.Combine(outputDir.FullName, "h2spec.junit.xml")).ToList();
                    H2SpecResult.AssertSuccessful(results);
                }
                finally
                {
                    // Dump logs and clean up resources
                    await docker.Containers.SafeDumpLogsAsync(serverContainerName, logger, cts.Token);
                    await docker.Containers.SafeDumpLogsAsync(specContainerName, logger, cts.Token);
                    await docker.Containers.SafeRemoveContainerAsync(serverContainerName, logger, cts.Token);
                    await docker.Containers.SafeRemoveContainerAsync(specContainerName, logger, cts.Token);
                    if (network != null)
                    {
                        await docker.Networks.SafeRemoveNetworkAsync(network.ID, logger, cts.Token);
                    }

                    try
                    {
                        Directory.Delete(tempDir.FullName);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
