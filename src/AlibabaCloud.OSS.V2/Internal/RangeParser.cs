using System;
using System.Collections.Generic;
using System.Globalization;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Helpers for reading byte offsets and sizes from HTTP response headers, ported
    /// from the Go SDK (parseOffsetAndSizeFromHeaders / ParseContentRange).
    /// </summary>
    internal static class RangeParser
    {
        private const string BytesPrefix = "bytes ";

        /// <summary>
        /// Derives the object offset and total size from Content-Length and Content-Range headers.
        /// Mirrors Go's parseOffsetAndSizeFromHeaders: size defaults to -1, and any malformed
        /// header yields (0, -1).
        /// </summary>
        public static (long Offset, long Size) ParseOffsetAndSizeFromHeaders(IDictionary<string, string> headers)
        {
            long size = -1;

            if (headers.TryGetValue("Content-Length", out var contentLength) && contentLength.Length != 0)
            {
                if (!long.TryParse(contentLength, NumberStyles.Integer, CultureInfo.InvariantCulture, out size))
                    return (0, -1);
            }

            if (!headers.TryGetValue("Content-Range", out var contentRange) || contentRange.Length == 0)
                return (0, size);

            if (!contentRange.StartsWith(BytesPrefix, StringComparison.Ordinal))
                return (0, -1);

            var dash = contentRange.IndexOf('-');
            if (dash < 0)
                return (0, -1);

            if (!long.TryParse(contentRange.Substring(6, dash - 6), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var offset))
                return (0, -1);

            var slash = contentRange.IndexOf('/');
            if (slash < 0)
                return (0, -1);

            var tsize = contentRange.Substring(slash + 1);
            if (tsize != "*")
            {
                if (!long.TryParse(tsize, NumberStyles.Integer, CultureInfo.InvariantCulture, out size))
                    return (0, -1);
            }

            return (offset, size);
        }

        /// <summary>
        /// Parses a Content-Range value of the form "bytes from-to/total". A total of "*" maps to -1.
        /// Returns false on any malformed input.
        /// </summary>
        public static bool TryParseContentRange(string s, out long from, out long to, out long total)
        {
            from = 0;
            to = 0;
            total = 0;

            if (string.IsNullOrEmpty(s) || !s.StartsWith(BytesPrefix, StringComparison.Ordinal))
                return false;

            var slash = s.IndexOf('/');
            if (slash < 0)
                return false;

            var dash = s.IndexOf('-');
            if (dash < 0 || slash < dash)
                return false;

            if (!long.TryParse(s.Substring(6, dash - 6), NumberStyles.Integer, CultureInfo.InvariantCulture, out from))
                return false;

            if (!long.TryParse(s.Substring(dash + 1, slash - dash - 1), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out to))
                return false;

            var last = s.Substring(slash + 1);
            if (last == "*")
            {
                total = -1;
                return true;
            }

            return long.TryParse(last, NumberStyles.Integer, CultureInfo.InvariantCulture, out total);
        }
    }
}
