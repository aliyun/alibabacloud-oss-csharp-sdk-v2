using System;
using System.IO;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// A read-only, seekable view over a fixed section [start, start+length) of an inner stream.
    /// Each instance owns its inner stream (typically a dedicated FileStream) so that multiple
    /// sections of the same file can be read concurrently. Seek is relative to the section start,
    /// which lets the transport layer rewind to position 0 when retrying a part upload.
    /// </summary>
    internal sealed class SectionReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _start;
        private readonly long _length;
        private long _position;
        private readonly bool _ownsInner;

        public SectionReadStream(Stream inner, long start, long length, bool ownsInner = true)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _start = start;
            _length = length;
            _ownsInner = ownsInner;
            _position = 0;
            _inner.Seek(_start, SeekOrigin.Begin);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = _length - _position;
            if (remaining <= 0)
                return 0;
            if (count > remaining)
                count = (int)remaining;
            _inner.Seek(_start + _position, SeekOrigin.Begin);
            var read = _inner.Read(buffer, offset, count);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => offset
            };
            if (target < 0) target = 0;
            if (target > _length) target = _length;
            _position = target;
            return _position;
        }

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing && _ownsInner)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
