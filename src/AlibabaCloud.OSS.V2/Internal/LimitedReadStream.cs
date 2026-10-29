using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Wraps a stream so that at most a fixed number of bytes can be read from it.
    /// </summary>
    internal sealed class LimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private long _remaining;

        public LimitedReadStream(Stream inner, long limit)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _remaining = limit;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining <= 0) return 0;
            if (count > _remaining)
                count = (int)Math.Min((long)int.MaxValue, _remaining);
            var n = _inner.Read(buffer, offset, count);
            _remaining -= n;
            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_remaining <= 0) return 0;
            if (count > _remaining)
                count = (int)Math.Min((long)int.MaxValue, _remaining);
            var n = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            _remaining -= n;
            return n;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
