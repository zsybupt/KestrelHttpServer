using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public static class ProjectBuilder
    {
        public static async Task<int> PublishProjectAsync(string solutionRelativePath, string framework, string output, ILogger logger, CancellationToken cancellationToken)
        {
            var solutionRoot = TestPathUtilities.GetSolutionRootDirectory("KestrelHttpServer");
            var projectPath = Path.Combine(solutionRoot, solutionRelativePath);
            Assert.True(File.Exists(projectPath), $"Project '{projectPath}' not found");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = $"publish --framework {framework} --output {output} {projectPath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true,
            };

            var exitTcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) => exitTcs.TrySetResult(process.ExitCode);
            process.ErrorDataReceived += (sender, args) => OnDataReceived("stderr", args.Data);
            process.OutputDataReceived += (sender, args) => OnDataReceived("stderr", args.Data);

            void OnDataReceived(string stream, string line)
            {
                if (line != null)
                {
                    logger.LogInformation("dotnet publish " + stream + ": {Message}", line);
                }
            }

            void OnCancelled()
            {
                process.Kill();
                exitTcs.TrySetCanceled();
            }

            using (cancellationToken.Register(OnCancelled))
            {
                logger.LogInformation("> '{Command} {Arguments}'", process.StartInfo.FileName, process.StartInfo.Arguments);
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                var exitCode = await exitTcs.Task;
                logger.LogInformation("> '{Command} {Arguments}' exited with exit code {ExitCode}", process.StartInfo.FileName, process.StartInfo.Arguments, exitCode);
                return exitCode;
            }
        }
    }
}
