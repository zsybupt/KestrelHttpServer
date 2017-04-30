// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal
{
    public class AdaptedPipeline
    {
        private const int MinAllocBufferSize = 2048;

        private readonly Stream _filteredStream;

        public AdaptedPipeline(
            Stream filteredStream,
            IPipe inputPipe,
            IPipe outputPipe)
        {
            Input = inputPipe;
            Output = outputPipe;

            _filteredStream = filteredStream;
        }

        public IPipe Input { get; }

        public IPipe Output { get; }

        public async Task RunAsync()
        {
            var inputTask = ReadInputAsync();
            var outputTask = WriteOutputAsync();

            await inputTask;
            await outputTask;
        }

        private async Task WriteOutputAsync()
        {
            try
            {
                while (true)
                {
                    var readResult = await Output.Reader.ReadAsync();
                    var buffer = readResult.Buffer;

                    try
                    {
                        if (buffer.IsEmpty && readResult.IsCompleted)
                        {
                            break;
                        }

                        if (buffer.IsEmpty)
                        {
                            await _filteredStream.FlushAsync();
                        }
                        else if (buffer.IsSingleSpan)
                        {
                            var array = buffer.First.GetArray();
                            await _filteredStream.WriteAsync(array.Array, array.Offset, array.Count);
                        }
                        else
                        {
                            foreach (var memory in buffer)
                            {
                                var array = memory.GetArray();
                                await _filteredStream.WriteAsync(array.Array, array.Offset, array.Count);
                            }
                        }
                    }
                    finally
                    {
                        Output.Reader.Advance(buffer.End);
                    }
                }
            }
            catch (Exception)
            {
                // TODO
            }
            finally
            {
                Output.Reader.Complete();
            }
        }

        private async Task ReadInputAsync()
        {
            int bytesRead;

            do
            {
                var block = Input.Writer.Alloc(MinAllocBufferSize);

                try
                {
                    var array = block.Buffer.GetArray();
                    try
                    {
                        bytesRead = await _filteredStream.ReadAsync(array.Array, array.Offset, array.Count);
                        block.Advance(bytesRead);
                    }
                    finally
                    {
                        await block.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Input.Writer.Complete(ex);

                    // Don't rethrow the exception. It should be handled by the Pipeline consumer.
                    return;
                }
            } while (bytesRead != 0);

            Input.Writer.Complete();
        }
    }
}
