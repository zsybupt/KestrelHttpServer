using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal
{
    public interface IApplication
    {
        Task ExecuteAsync(string connectionId, IConnectionInformation info, IPipeReader reader, IPipeWriter writer);
    }
}
