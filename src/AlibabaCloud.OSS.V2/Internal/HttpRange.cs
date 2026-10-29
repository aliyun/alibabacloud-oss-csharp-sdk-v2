using System;
using System.Globalization;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// A byte range within an object, expressed as a start offset and a count.
    /// A <see cref="Count"/> of 0 means "to the end of the object".
    /// A suffix range (e.g. <c>bytes=-500</c>) is represented by <see cref="IsSuffix"/>
    /// and <see cref="SuffixLength"/>.
    /// </summary>
    internal readonly struct HttpRange
    {
        public HttpRange(long offset, long count)
        {
            Offset = offset;
            Count = count;
            IsSuffix = false;
            SuffixLength = 0;
        }

        private HttpRange(long suffixLength, bool _)
        {
            Offset = 0;
            Count = 0;
            IsSuffix = true;
            SuffixLength = suffixLength;
        }

        /// <summary>The start offset of the range.</summary>
        public long Offset { get; }

        /// <summary>
        /// The number of bytes requested. 0 means "from <see cref="Offset"/> to the end".
        /// </summary>
        public long Count { get; }

        /// <summary>Whether this is a suffix range such as <c>bytes=-500</c>.</summary>
        public bool IsSuffix { get; }

        /// <summary>The suffix length in bytes. Only meaningful when <see cref="IsSuffix"/> is true.</summary>
        public long SuffixLength { get; }

        /// <summary>Alias for <see cref="Offset"/>, used by the high-level transfer components.</summary>
        public long Start => Offset;

        /// <summary>Creates a suffix range that requests the last <paramref name="length"/> bytes.</summary>
        public static HttpRange Suffix(long length) => new(length, true);

        /// <summary>
        /// Formats the range as an HTTP <c>Range</c> header value ("bytes=start-end").
        /// Returns null when no range is specified.
        /// </summary>
        public string? FormatHttpRange()
        {
            if (IsSuffix)
            {
                if (SuffixLength <= 0)
                    return null;
                return "bytes=-" + (SuffixLength - 1).ToString(CultureInfo.InvariantCulture);
            }

            if (Offset == 0 && Count == 0)
                return null;

            var end = Count > 0
                ? (Offset + Count - 1).ToString(CultureInfo.InvariantCulture)
                : "";
            return "bytes=" + Offset.ToString(CultureInfo.InvariantCulture) + "-" + end;
        }

        /// <summary>
        /// Alias for <see cref="FormatHttpRange"/>, used by the high-level transfer components.
        /// </summary>
        public string? Format() => FormatHttpRange();
    }
}
