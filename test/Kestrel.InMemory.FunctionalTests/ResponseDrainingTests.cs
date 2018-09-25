// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.InMemory.FunctionalTests.TestTransport;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Tests
{
    public class ResponseDrainingTests : TestApplicationErrorLoggerLoggedTest
    {
        [Fact]
        public async Task ConnectionClosedWhenResponseNotDrainedAtMinimumDataRate()
        {
            var testContext = new TestServiceContext(LoggerFactory);
            var minRate = testContext.ServerOptions.Limits.MinResponseDataRate;
            var heartbeatManager = new HeartbeatManager(testContext.ConnectionManager);

            using (var server = new TestServer(context =>
            {
                context.Response.ContentLength = 11;

                return context.Response.WriteAsync("Hello World");
            }, testContext))
            {
                using (var connection = server.CreateConnection())
                {
                    await connection.Send(
                        "GET / HTTP/1.1",
                        "Host:",
                        "Connection: close",
                        "",
                        "");

                    // Read all but the last byte of the response.
                    await connection.Receive(
                        "HTTP/1.1 200 OK",
                        "Connection: close",
                        $"Date: {testContext.DateHeaderValue}",
                        "Content-Length: 11",
                        "",
                        "Hello Worl");

                    await Task.Delay(1000);

                    heartbeatManager.OnHeartbeat(testContext.SystemClock.UtcNow);

                    Assert.Null(connection.AbortReason);

                    testContext.MockSystemClock.UtcNow +=
                        Heartbeat.Interval +
                        TimeSpan.FromSeconds(testContext.ServerOptions.Limits.MaxResponseBufferSize.Value * 2 / minRate.BytesPerSecond) +
                        TimeSpan.FromTicks(1);

                    heartbeatManager.OnHeartbeat(testContext.SystemClock.UtcNow);

                    Assert.NotNull(connection.AbortReason);
                    Assert.Equal(CoreStrings.ConnectionTimedBecauseResponseMininumDataRateNotSatisfied, connection.AbortReason.Message);
                }
            }
        }
    }
}
