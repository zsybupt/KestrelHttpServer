using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Features
{
    // TODO: Move this to HttpAbstractions
    interface IHttpResponseTrailersFeature
    {
        IHeaderDictionary Trailers { get; set; }
    }
}
