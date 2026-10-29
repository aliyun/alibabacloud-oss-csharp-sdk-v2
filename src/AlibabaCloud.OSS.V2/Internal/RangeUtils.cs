using System;
using System.Globalization;

namespace AlibabaCloud.OSS.V2.Internal
{
    internal static class RangeUtils
    {
        /// <summary>
        /// Parses an HTTP Range header value ("bytes=start-end"). Returns null when the value is
        /// malformed or contains multiple ranges. Supports suffix ranges ("bytes=-suffix").
        /// </summary>
        public static HttpRange? ParseRange(string? s)
        {
            const string preamble = "bytes=";
            if (string.IsNullOrEmpty(s) || !s!.StartsWith(preamble, StringComparison.Ordinal))
                return null;

            var body = s.Substring(preamble.Length);
            if (body.IndexOf(',') >= 0)
                return null;

            var dash = body.IndexOf('-');
            if (dash < 0)
                return null;

            var startStr = body.Substring(0, dash).Trim();
            var endStr = body.Substring(dash + 1).Trim();

            // Suffix range: bytes=-suffix
            if (startStr.Length == 0)
            {
                if (endStr.Length == 0)
                    return null;
                if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var suffix) || suffix < 0)
                    return null;
                return HttpRange.Suffix(suffix);
            }

            if (!long.TryParse(startStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start) || start < 0)
                return null;

            // Open-ended range: bytes=start-
            if (endStr.Length == 0)
                return new HttpRange(start, 0);

            if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var end) || end < 0)
                return null;

            // Reversed or empty range.
            if (end < start)
                return null;

            // Avoid integer overflow when computing count. The only case where end - start + 1
            // can overflow is start == 0 && end == long.MaxValue.
            if (start == 0 && end == long.MaxValue)
                return null;

            var count = end - start + 1;
            return new HttpRange(start, count);
        }
    }
}
