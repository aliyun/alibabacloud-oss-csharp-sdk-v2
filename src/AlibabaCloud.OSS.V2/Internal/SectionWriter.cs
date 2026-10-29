using System;
using System.IO;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Thread-safe positioned writer over a single <see cref="FileStream"/>. Used by the
    /// Downloader to write concurrently downloaded ranges to their target offsets. A lock is
    /// used (rather than RandomAccess) so behavior is identical across all target frameworks,
    /// including netstandard2.0.
    /// </summary>
    internal sealed class SectionWriter
    {
        private readonly FileStream _stream;
        private readonly object _lock = new();

        public SectionWriter(FileStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void WriteAt(byte[] buffer, int offset, int count, long position)
        {
            lock (_lock)
            {
                _stream.Seek(position, SeekOrigin.Begin);
                _stream.Write(buffer, offset, count);
            }
        }
    }
}
