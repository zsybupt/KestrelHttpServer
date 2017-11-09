using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        [ConditionalFact]
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

                try
                {

                    // Create a network for the test
                    network = await docker.Networks.CreateTestBridgeNetworkAsync(runName, logger, cts.Token);

                    // Start the container using 'tail -f /dev/null' so we can exec into it
                    // Also, host bind-mount the project directory.
                    var projectRoot = @"C:\Users\anurse\Code\aspnet\KestrelHttpServer\samples\Http2SampleApp\bin\Debug\netcoreapp2.0\publish";
                    await docker.Containers.RunContainerAsync(serverContainerName, new CreateContainerParameters()
                        .WithImage("microsoft/aspnetcore:2.0")
                        .WithCmd("/app/scripts/container-start.sh")
                        .WithBinds($"{projectRoot}:/app")
                        .WithNetwork(network.ID)
                        .WithHttpHealthcheck("https://localhost:5000"), logger, cts.Token);

                    // Wait for the container to become healthy
                    await docker.Containers.WaitForHealthyAsync(serverContainerName, logger, cts.Token);
                    logger.LogInformation("Server container started");

                    // Setup a temp dir
                    var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"Kestrel_{nameof(H2SpecTest)}_{Guid.NewGuid().ToString("N")}"));

                    // Run the h2spec container
                    await docker.Containers.RunContainerAsync(specContainerName, new CreateContainerParameters()
                        .WithImage("summerwind/h2spec")
                        .WithCmd("--host", serverContainerName, "--port", "5000", "-k", "-t", "-j", "/tmp/kestrel_tests/h2spec.junit.xml")
                        .WithBinds($"{tempDir.FullName}:/tmp/kestrel_tests")
                        .WithNetwork(network.ID), logger, cts.Token);

                    // Wait for h2spec to stop
                    await docker.Containers.WaitContainerAsync(specContainerName, logger, cts.Token);

                    // Stop the server
                    await docker.Containers.KillContainerAndWaitForExitAsync(serverContainerName, logger, cts.Token);

                    var doc = XDocument.Load(Path.Combine(tempDir.FullName, "h2spec.junit.xml"));
                    var failures = new StringBuilder();
                    foreach (var testCase in doc.Descendants("testcase"))
                    {
                        var package = testCase.Attribute("package")?.Value;
                        var className = testCase.Attribute("className")?.Value;
                        var time = testCase.Attribute("time")?.Value;
                        var error = testCase.Element("error")?.Value;
                        if (error != null)
                        {
                            failures.AppendLine($"  * {package}.{className} (duration: {time}s)");
                            foreach (var line in error.Split('\n')) // Because the test runs in docker, it's always '\n'
                            {
                                failures.AppendLine($"    {line}");
                            }
                        }
                    }
                    Assert.True(failures.Length == 0, $"h2spec Failures: {Environment.NewLine}{failures.ToString()}");
                }
                finally
                {
                    // Dump logs and clean up resources
                    await docker.Containers.SafeDumpLogsAsync(serverContainerName, logger, cts.Token);
                    await docker.Containers.SafeDumpLogsAsync(serverContainerName, logger, cts.Token);
                    await docker.Containers.SafeRemoveContainerAsync(serverContainerName, logger, cts.Token);
                    await docker.Containers.SafeRemoveContainerAsync(specContainerName, logger, cts.Token);
                    if (network != null)
                    {
                        await docker.Networks.SafeRemoveNetworkAsync(network.ID, logger, cts.Token);
                    }
                }
            }
        }
    }
}
