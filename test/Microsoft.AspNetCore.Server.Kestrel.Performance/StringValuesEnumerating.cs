// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Server.Kestrel.Performance
{
    [Config(typeof(CoreConfig))]
    public class StringValuesEnumerating
    {
        private const int InnerLoopCount = 512;

        private StringValues NoStringValues;
        private StringValues SingleStringValue;
        private StringValues MultipleStringValues;

        [Benchmark(Baseline = true, OperationsPerInvoke = InnerLoopCount)]
        public int ForeachNoStringValues()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                foreach (var value in NoStringValues)
                {
                    totalHeaderChars += value.Length;
                }
            }

            return totalHeaderChars;
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public int ForeachSingleStringValue()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                foreach (var value in SingleStringValue)
                {
                    totalHeaderChars += value.Length;
                }
            }

            return totalHeaderChars;
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public int ForeachMultipleStringValues()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                foreach (var value in MultipleStringValues)
                {
                    totalHeaderChars += value.Length;
                }
            }

            return totalHeaderChars;
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public int ForNoStringValues()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                var count = NoStringValues.Count;
                for (var j = 0; j < count; j++)
                {
                    totalHeaderChars += NoStringValues[j].Length;
                }
            }

            return totalHeaderChars;
        }


        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public int ForSingleStringValue()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                var count = SingleStringValue.Count;
                for (var j = 0; j < count; j++)
                {
                    totalHeaderChars += SingleStringValue[j].Length;
                }
            }

            return totalHeaderChars;
        }

        [Benchmark(OperationsPerInvoke = InnerLoopCount)]
        public int ForMultipleStringValues()
        {
            var totalHeaderChars = 0;

            for (var i = 0; i < InnerLoopCount; i++)
            {
                var count = MultipleStringValues.Count;
                for (var j = 0; j < count; j++)
                {
                    totalHeaderChars += MultipleStringValues[j].Length;
                }
            }

            return totalHeaderChars;
        }

        [Setup]
        public void Setup()
        {
            NoStringValues = new StringValues();
            SingleStringValue = new StringValues("max-age=0");
            MultipleStringValues = new StringValues(new[] { "text/html, application/xhtml+xml, image/jxr, */*", "en-US,en-GB;q=0.7,en;q=0.3" });
        }
    }
}
