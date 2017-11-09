using System;
using Microsoft.AspNetCore.Testing.xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class LocalDockerAvailableSkipConditionAttribute : Attribute, ITestCondition
    {
        public bool IsMet => DockerUtils.HasLocalDocker;

        public string SkipReason => "There is no local docker daemon on this machine or it is not currently running.";
    }
}
