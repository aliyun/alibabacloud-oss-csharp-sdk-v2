using System;
using System.IO;

namespace AlibabaCloud.OSS.V2.Internal {
    internal class ProgressStream : Stream {
        private ProgressFunc _fn;
        private long _written;
        private long _lwritten;
        private long _total;
        private bool _adjust;

        public ProgressStream(ProgressFunc fn, long total = -1) {
            _fn = fn;
            _total = total;
            _written = 0;
            _lwritten = 0;
            _adjust = false;
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _lwritten;

        public override long Position { 
            get => _lwritten; 
            set => throw new System.NotImplementedException(); }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (origin == SeekOrigin.Begin && offset == 0) {
                _lwritten = _written;
                _written = 0;
                _adjust = true;
                return 0;
            }
            throw new System.NotImplementedException($"seek to beginning only, offset:{offset}, origin:{origin}.");
        }

        public override void SetLength(long value) {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _written += count;
            if (_written > _lwritten) {
                if (_adjust) {
                    _adjust = false;
                    count = (int)Math.Min(_written - _lwritten, count);
                }
                _fn(count, _written, _total);
            }
        }
    }
}
