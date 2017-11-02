// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Server.Kestrel
{
    internal class KestrelServerOptionsConfigureOptions : IConfigureOptions<KestrelServerOptions>
    {
        private readonly IConfiguration _configuration;

        public KestrelServerOptionsConfigureOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(KestrelServerOptions options)
        {
            if (_configuration != null)
            {
                // TODO: Load simple config values? Use Config Binder?
                // This is just for prototype/demostration purposes
                var addServerHeaderValue = _configuration["AddServerHeader"];

                var certificateSection = _configuration.GetSection("Certificate");
                var endpointsSection = _configuration.GetSection("Endpoints");
                if (endpointsSection.Exists())
                {
                    foreach (var endpointGroup in endpointsSection.GetChildren())
                    {
                        // Urls can be a single value...
                        if (endpointGroup["Urls"] != null)
                        {
                            CreateListenOptions(options, endpointGroup["Urls"], endpointGroup, certificateSection);
                        }
                        else
                        {
                            // ... or multiple values
                            var urlsSection = endpointGroup.GetSection("Urls");
                            if (urlsSection.Exists())
                            {
                                foreach (var url in urlsSection.GetChildren())
                                {
                                    CreateListenOptions(options, url.Value, endpointGroup, certificateSection);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateListenOptions(KestrelServerOptions serverOptions, string url, IConfigurationSection endpointOptions, IConfigurationSection globalCertificateSection)
        {
            var address = ServerAddress.FromUrl(url);

            void ConfigureListenOptions(ListenOptions listenOptions)
            {
                if (address.Scheme.Equals("https", StringComparison.Ordinal))
                {
                    var certSection = endpointOptions.GetSection("Certificate");
                    if(!certSection.Exists())
                    {
                        certSection = globalCertificateSection;
                    }

                    if(!certSection.Exists())
                    {
                        throw new InvalidOperationException("TODO: No cert found. Use DefaultHttpsProvider here?");
                    }

                    // TODO: Expand cert selection options
                    var path = certSection["Path"];
                    var password = certSection["Password"];
                    listenOptions.UseHttps(path, password);
                }

            }

            if (string.Equals(address.Host, "*", StringComparison.Ordinal))
            {
                serverOptions.Listen(IPAddress.Any, address.Port, ConfigureListenOptions);
                serverOptions.Listen(IPAddress.IPv6Any, address.Port, ConfigureListenOptions);
            }
            else if (string.Equals(address.Host, "localhost", StringComparison.Ordinal))
            {
                serverOptions.Listen(IPAddress.Loopback, address.Port, ConfigureListenOptions);
                serverOptions.Listen(IPAddress.IPv6Loopback, address.Port, ConfigureListenOptions);
            }
            else if (IPAddress.TryParse(address.Host, out var ipAddress))
            {
                serverOptions.Listen(ipAddress, address.Port, ConfigureListenOptions);
            }
            else
            {
                // We don't support DNS names
                throw new InvalidOperationException($"DNS names (other than localhost) are not supported in the Urls configuration setting: {address.Host}");
            }
        }
    }
}
