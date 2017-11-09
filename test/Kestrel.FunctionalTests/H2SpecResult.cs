using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    public class H2SpecResult
    {
        public string TestCase { get; }
        public string Description { get; }
        public string Error { get; }
        public TimeSpan Duration { get; }

        public bool Success => string.IsNullOrEmpty(Error);

        public H2SpecResult(string testCase, string description, TimeSpan duration, string error)
        {
            TestCase = testCase;
            Description = description;
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

                result.Add(new H2SpecResult(package, className, time, error));
            }

            return result;
        }

        public static void AssertSuccessful(IList<H2SpecResult> results, params string[] expectedFailures)
        {
            var expectedFailuresSet = new HashSet<string>(expectedFailures);
            var failures = new StringBuilder();
            foreach (var failure in results.Where(r => !r.Success && !expectedFailuresSet.Contains(r.TestCase)))
            {
                failures.AppendLine($"  * {failure.TestCase} {failure.Description} (duration: {failure.Duration.TotalSeconds:0.00}s)");
                foreach (var line in failure.Error.Split('\n')) // Because the test runs in docker, it's always '\n'
                {
                    failures.AppendLine($"    {line}");
                }
            }
            Assert.True(failures.Length == 0, $"h2spec Failures: {Environment.NewLine}{failures.ToString()}");
        }
    }
}
