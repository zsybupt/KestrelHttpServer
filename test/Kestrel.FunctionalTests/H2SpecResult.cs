using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public class H2SpecResult
    {
        public string TestCase { get; }
        public string Error { get; }
        public TimeSpan Duration { get; }

        public bool Success => string.IsNullOrEmpty(Error);

        public H2SpecResult(string testCase, TimeSpan duration, string error)
        {
            TestCase = testCase;
            Duration = duration;
            Error = error;
        }

        public static IList<H2SpecResult> LoadResults(string filePath) => LoadResults(XDocument.Load(filePath));
        public static IList<H2SpecResult> LoadResults(XDocument doc)
        {
            var failures = new StringBuilder();
            var result = new List<H2SpecResult>();
            foreach (var testCase in doc.Descendants("testcase"))
            {
                var package = testCase.Attribute("package")?.Value;
                var className = testCase.Attribute("classname")?.Value;
                var time = TimeSpan.FromSeconds(Double.Parse(testCase.Attribute("time").Value));
                var error = testCase.Element("error")?.Value;

                result.Add(new H2SpecResult($"{package} {className}", time, error));
            }

            return result;
        }

        public static void AssertSuccessful(IList<H2SpecResult> results, ILogger logger, params string[] expectedFailures)
        {
            var expectedFailuresSet = new HashSet<string>(expectedFailures);
            var failures = new StringBuilder();
            var success = true;
            foreach (var failure in results.Where(r => !r.Success))
            {
                if (expectedFailuresSet.Contains(failure.TestCase))
                {
                    logger.LogWarning("Suppressed Failure: {TestCase} - {Error}", failure.TestCase, failure.Error);
                }
                else
                {
                    logger.LogError("Failure: {TestCase} - {Error}", failure.TestCase, failure.Error);
                    failures.AppendLine($"  * {failure.TestCase} (duration: {failure.Duration.TotalSeconds:0.00}s)");
                    foreach (var line in failure.Error.Split('\n')) // Because the test runs in docker, it's always '\n'
                    {
                        failures.AppendLine($"    {line}");
                    }
                }
            }

            Assert.True(success, $"h2spec Failures: {Environment.NewLine}{failures.ToString()}");
        }
    }
}
