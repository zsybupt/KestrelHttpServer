// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace SampleApp
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(LogLevel.Trace);
            //var logger = loggerFactory.CreateLogger("Default");

            app.Run(async context =>
            {
                var connectionFeature = context.Connection;
                //logger.LogDebug($"Peer: {connectionFeature.RemoteIpAddress?.ToString()}:{connectionFeature.RemotePort}"
                //    + $"{Environment.NewLine}"
                //    + $"Sock: {connectionFeature.LocalIpAddress?.ToString()}:{connectionFeature.LocalPort}");

                var response = $"hello, world{Environment.NewLine}";
                context.Response.ContentLength = response.Length;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(response);
            });
        }

        public static void Main(string[] args)
        {
            var sw = new Stopwatch();

            using (var client = new HttpClient())
            {
                sw.Start();
                client.GetAsync("http://google.com").Wait();
                sw.Stop();
            }
            Console.WriteLine("Google start time: {0}", sw.ElapsedMilliseconds);

            using (Run1(args))
            {
            }

            using (Run2(args))
            using (var client = new HttpClient())
            {
                sw.Start();
                Get1(client);
                sw.Stop();
            }

            Console.WriteLine("First start time: {0}", sw.ElapsedMilliseconds);


            using (Run3(args))
            using (var client = new HttpClient())
            {
                sw.Restart();
                Get2(client);
                sw.Stop();
            }

            Console.WriteLine("Second start time: {0}", sw.ElapsedMilliseconds);


            using (Run4(args))
            using (var client = new HttpClient())
            {
                sw.Restart();
                Get3(client);
                sw.Stop();
            }

            Console.WriteLine("Third start time: {0}", sw.ElapsedMilliseconds);

            //Console.Read();
        }

        private static void Get(HttpClient client)
        {
            client.GetAsync("http://[::1]:5000/").Wait();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IDisposable Run1(string[] args)
        {
            return Main2(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IDisposable Run2(string[] args)
        {
            return Main2(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IDisposable Run3(string[] args)
        {
            return Main2(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IDisposable Run4(string[] args)
        {
            return Main2(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Get1(HttpClient client)
        {
            Get(client);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Get2(HttpClient client)
        {
            Get(client);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Get3(HttpClient client)
        {
            Get(client);
        }

        public static IDisposable Main2(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    // options.ThreadCount = 4;
                    options.NoDelay = true;
                    //options.UseHttps("testCert.pfx", "testPassword");
                    //options.UseConnectionLogging();
                })
                .UseUrls("http://localhost:5000", "https://localhost:5001")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            // The following section should be used to demo sockets
            //var addresses = application.GetAddresses();
            //addresses.Clear();
            //addresses.Add("http://unix:/tmp/kestrel-test.sock");

            host.Start();
            return host;
        }
    }
}