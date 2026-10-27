using System.Globalization;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// A byte range within an object, expressed as a start offset and a count.
    /// A <see cref="Count"/> of 0 means "to the end of the object".
    /// </summary>
    internal readonly struct HttpRange
    {
        public HttpRange(long offset, long count)
        {
            Offset = offset;
            Count = count;
        }

        public long Offset { get; }

        public long Count { get; }

        /// <summary>
        /// Formats the range as an HTTP <c>Range</c> header value ("bytes=start-end").
        /// Returns null when both offset and count are zero (no range specified).
        /// </summary>
        public string? FormatHttpRange()
        {
            if (Offset == 0 && Count == 0)
                return null;

            var end = Count > 0
                ? (Offset + Count - 1).ToString(CultureInfo.InvariantCulture)
                : "";
            return "bytes=" + Offset.ToString(CultureInfo.InvariantCulture) + "-" + end;
        }
    }
}
