// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http
{
    public class HttpParser : IHttpParser
    {
        public HttpParser(IKestrelTrace log)
        {
            Log = log;
        }

        private IKestrelTrace Log { get; }

        // byte types don't have a data type annotation so we pre-cast them; to avoid in-place casts
        private const byte ByteCR = (byte)'\r';
        private const byte ByteLF = (byte)'\n';
        private const byte ByteColon = (byte)':';
        private const byte ByteSpace = (byte)' ';
        private const byte ByteTab = (byte)'\t';
        private const byte ByteQuestionMark = (byte)'?';
        private const byte BytePercentage = (byte)'%';

        public bool ParseRequestLine(IHttpRequestLineHandler handler, ReadableBuffer buffer, out ReadCursor consumed, out ReadCursor examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            // Prepare the first span
            var span = buffer.First.Span;
            var lineIndex = span.IndexOf(ByteLF);
            if (lineIndex >= 0)
            {
                consumed = buffer.Move(consumed, lineIndex + 1);
                span = span.Slice(0, lineIndex + 1);
            }
            else if (buffer.IsSingleSpan)
            {
                // No request line end
                return false;
            }
            else if (TryGetNewLine(ref buffer, out var found))
            {
                span = buffer.Slice(consumed, found).ToSpan();
                consumed = found;
            }
            else
            {
                // No request line end
                return false;
            }

            // Parse the span
            ParseRequestLine(handler, span);

            examined = consumed;
            return true;
        }

        private void ParseRequestLine(IHttpRequestLineHandler handler, Span<byte> data)
        {
            var customMethod = default(Span<byte>);
            // Get Method and set the offset
            var method = HttpUtilities.GetKnownMethod(data, out var offset);
            if (method == HttpMethod.Custom)
            {
                customMethod = GetUnknownMethod(data, out offset);
            }

            // Skip space
            offset++;

            byte ch = 0;
            // Target = Path and Query
            var pathEncoded = false;
            var pathStart = -1;
            for (; offset < data.Length; offset++)
            {
                ch = data[offset];
                if (ch == ByteSpace)
                {
                    if (pathStart == -1)
                    {
                        // Empty path is illegal
                        RejectRequestLine(data);
                    }

                    break;
                }
                else if (ch == ByteQuestionMark)
                {
                    if (pathStart == -1)
                    {
                        // Empty path is illegal
                        RejectRequestLine(data);
                    }

                    break;
                }
                else if (ch == BytePercentage)
                {
                    if (pathStart == -1)
                    {
                        // Path starting with % is illegal
                        RejectRequestLine(data);
                    }

                    pathEncoded = true;
                }
                else if (pathStart == -1)
                {
                    pathStart = offset;
                }
            }

            if (pathStart == -1)
            {
                // Start of path not found
                RejectRequestLine(data);
            }

            var pathBuffer = data.Slice(pathStart, offset - pathStart);

            // Query string
            var queryStart = offset;
            if (ch == ByteQuestionMark)
            {
                // We have a query string
                for (; offset < data.Length; offset++)
                {
                    ch = data[offset];
                    if (ch == ByteSpace)
                    {
                        break;
                    }
                }
            }

            // End of query string not found
            if (offset == data.Length)
            {
                RejectRequestLine(data);
            }

            var targetBuffer = data.Slice(pathStart, offset - pathStart);
            var query = data.Slice(queryStart, offset - queryStart);

            // Consume space
            offset++;

            // Version
            var httpVersion = HttpUtilities.GetKnownVersion(data.Slice(offset));
            if (httpVersion == HttpVersion.Unknown)
            {
                if (data[offset] == ByteCR || data[data.Length - 2] != ByteCR)
                {
                    // If missing delimiter or CR before LF, reject and log entire line
                    RejectRequestLine(data);
                }
                else
                {
                    // else inform HTTP version is unsupported.
                    RejectUnknownVersion(data.Slice(offset, data.Length - offset - 2));
                }
            }

            // After version's 8 bytes and CR, expect LF
            if (data[offset + 8 + 1] != ByteLF)
            {
                RejectRequestLine(data);
            }

            handler.OnStartLine(method, httpVersion, targetBuffer, pathBuffer, query, customMethod, pathEncoded);
        }

        public bool ParseHeaders(IHttpHeadersHandler handler, ReadableBuffer buffer, out ReadCursor consumed, out ReadCursor examined, out int consumedBytes)
        {
            consumed = buffer.Start;
            examined = buffer.End;
            consumedBytes = 0;

            var bufferEnd = buffer.End;

            var reader = new ReadableBufferReader(buffer);
            var start = default(ReadableBufferReader);
            var done = false;

            try
            {
                while (!reader.End)
                {
                    var span = reader.Span;
                    var remaining = span.Length - reader.Index;

                    while (remaining > 0)
                    {
                        var index = reader.Index;
                        int ch1;
                        int ch2;

                        // Fast path, we're still looking at the same span
                        if (remaining >= 2)
                        {
                            ch1 = span[index];
                            ch2 = span[index + 1];
                        }
                        else
                        {
                            // Store the reader before we look ahead 2 bytes (probably straddling
                            // spans)
                            start = reader;

                            // Possibly split across spans
                            ch1 = reader.Take();
                            ch2 = reader.Take();
                        }

                        if (ch1 == ByteCR)
                        {
                            // Check for final CRLF.
                            if (ch2 == -1)
                            {
                                // Reset the reader so we don't consume anything
                                reader = start;
                                return false;
                            }
                            else if (ch2 == ByteLF)
                            {
                                // If we got 2 bytes from the span directly so skip ahead 2 so that
                                // the reader's state matches what we expect
                                if (index == reader.Index)
                                {
                                    reader.Skip(2);
                                }

                                done = true;
                                return true;
                            }

                            // Headers don't end in CRLF line.
                            RejectRequest(RequestRejectionReason.InvalidRequestHeadersNoCRLF);
                        }

                        // We moved the reader so look ahead 2 bytes so reset both the reader
                        // and the index
                        if (index != reader.Index)
                        {
                            reader = start;
                            index = reader.Index;
                        }

                        var endIndex = span.Slice(index, remaining).IndexOf(ByteLF);
                        var length = 0;

                        if (endIndex != -1)
                        {
                            length = endIndex + 1;

                            TakeSingleHeader(span.Slice(index, length), handler);
                        }
                        else
                        {
                            var current = reader.Cursor;

                            // Split buffers
                            if (ReadCursorOperations.Seek(current, bufferEnd, out var lineEnd, ByteLF) == -1)
                            {
                                // Not there
                                return false;
                            }

                            // Make sure LF is included in lineEnd
                            lineEnd = buffer.Move(lineEnd, 1);
                            var headerSpan = buffer.Slice(current, lineEnd).ToSpan();
                            length = headerSpan.Length;

                            TakeSingleHeader(headerSpan, handler);

                            // We're going to the next span after this since we know we crossed spans here
                            // so mark the remaining as equal to the headerSpan so that we end up at 0
                            // on the next iteration
                            remaining = length;
                        }

                        // Skip the reader forward past the header line
                        reader.Skip(length);
                        remaining -= length;
                    }
                }

                return false;
            }
            finally
            {
                consumed = reader.Cursor;
                consumedBytes = reader.ConsumedBytes;

                if (done)
                {
                    examined = consumed;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindEndOfName(Span<byte> headerLine)
        {
            var index = 0;
            var sawWhitespace = false;
            for (; index < headerLine.Length; index++)
            {
                var ch = headerLine[index];
                if (ch == ByteColon)
                {
                    break;
                }
                if (ch == ByteTab || ch == ByteSpace || ch == ByteCR)
                {
                    sawWhitespace = true;
                }
            }

            if (index == headerLine.Length || sawWhitespace)
            {
                RejectRequestHeader(headerLine);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TakeSingleHeader(Span<byte> headerLine, IHttpHeadersHandler handler)
        {
            // Skip CR, LF from end position
            var valueEnd = headerLine.Length - 3;
            var nameEnd = FindEndOfName(headerLine);

            if (headerLine[valueEnd + 2] != ByteLF)
            {
                RejectRequestHeader(headerLine);
            }
            if (headerLine[valueEnd + 1] != ByteCR)
            {
                RejectRequestHeader(headerLine);
            }

            // Skip colon from value start
            var valueStart = nameEnd + 1;
            // Ignore start whitespace
            for (; valueStart < valueEnd; valueStart++)
            {
                var ch = headerLine[valueStart];
                if (ch != ByteTab && ch != ByteSpace && ch != ByteCR)
                {
                    break;
                }
                else if (ch == ByteCR)
                {
                    RejectRequestHeader(headerLine);
                }
            }

            // Check for CR in value
            var valueBuffer = headerLine.Slice(valueStart, valueEnd - valueStart + 1);
            if (valueBuffer.IndexOf(ByteCR) >= 0)
            {
                RejectRequestHeader(headerLine);
            }

            // Ignore end whitespace
            var lengthChanged = false;
            for (; valueEnd >= valueStart; valueEnd--)
            {
                var ch = headerLine[valueEnd];
                if (ch != ByteTab && ch != ByteSpace)
                {
                    break;
                }

                lengthChanged = true;
            }

            if (lengthChanged)
            {
                // Length changed
                valueBuffer = headerLine.Slice(valueStart, valueEnd - valueStart + 1);
            }

            var nameBuffer = headerLine.Slice(0, nameEnd);

            handler.OnHeader(nameBuffer, valueBuffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryGetNewLine(ref ReadableBuffer buffer, out ReadCursor found)
        {
            var start = buffer.Start;
            if (ReadCursorOperations.Seek(start, buffer.End, out found, ByteLF) != -1)
            {
                // Move 1 byte past the \n
                found = buffer.Move(found, 1);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Span<byte> GetUnknownMethod(Span<byte> data, out int methodLength)
        {
            methodLength = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var ch = data[i];

                if (ch == ByteSpace)
                {
                    if (i == 0)
                    {
                        RejectRequestLine(data);
                    }

                    methodLength = i;
                    break;
                }
                else if (!IsValidTokenChar((char)ch))
                {
                    RejectRequestLine(data);
                }
            }

            return data.Slice(0, methodLength);
        }

        private static bool IsValidTokenChar(char c)
        {
            // Determines if a character is valid as a 'token' as defined in the
            // HTTP spec: https://tools.ietf.org/html/rfc7230#section-3.2.6
            return
                (c >= '0' && c <= '9') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                c == '!' ||
                c == '#' ||
                c == '$' ||
                c == '%' ||
                c == '&' ||
                c == '\'' ||
                c == '*' ||
                c == '+' ||
                c == '-' ||
                c == '.' ||
                c == '^' ||
                c == '_' ||
                c == '`' ||
                c == '|' ||
                c == '~';
        }

        private void RejectRequest(RequestRejectionReason reason)
            => throw BadHttpRequestException.GetException(reason);

        private void RejectRequestLine(Span<byte> requestLine)
            => throw GetInvalidRequestException(RequestRejectionReason.InvalidRequestLine, requestLine);

        private void RejectRequestHeader(Span<byte> headerLine)
            => throw GetInvalidRequestException(RequestRejectionReason.InvalidRequestHeader, headerLine);

        private void RejectUnknownVersion(Span<byte> version)
            => throw GetInvalidRequestException(RequestRejectionReason.UnrecognizedHTTPVersion, version);

        private BadHttpRequestException GetInvalidRequestException(RequestRejectionReason reason, Span<byte> detail)
            => BadHttpRequestException.GetException(
                reason,
                Log.IsEnabled(LogLevel.Information)
                    ? detail.GetAsciiStringEscaped(Constants.MaxExceptionDetailSize)
                    : string.Empty);

        public void Reset()
        {
        }
    }
}
