using System;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    internal class LoggerProgress : IProgress<JSONMessage>
    {
        private ILogger _logger;
        private readonly string _prefix;

        public LoggerProgress(ILogger logger, string prefix)
        {
            _logger = logger;
            _prefix = prefix;
        }

        public void Report(JSONMessage value)
        {
            if(!string.IsNullOrEmpty(value.ErrorMessage))
            {
                _logger.LogError(_prefix + "> Error {Code}: {Message}", value.Error.Code, value.Error.Message);
            }
            else
            {
                _logger.LogInformation(_prefix + "> {Message}", value.ProgressMessage);
            }
        }
    }
}
