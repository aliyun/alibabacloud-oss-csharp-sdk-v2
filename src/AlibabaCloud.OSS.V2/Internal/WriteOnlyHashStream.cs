using System.IO;

namespace AlibabaCloud.OSS.V2.Internal {
    internal class WriteOnlyHashStream : Stream {
        private IHash _hash;
        private long _offset;

        public WriteOnlyHashStream(IHash hash) {
            _hash = hash;
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _offset;

        public override long Position { 
            get => _offset; 
            set => throw new System.NotImplementedException(); }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (origin == SeekOrigin.Begin && offset == 0) {
                _hash.Reset();
                return 0;
            }
            throw new System.NotImplementedException($"seek to beginning only, offset:{offset}, origin:{origin}.");
        }

        public override void SetLength(long value) {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _hash.Update(buffer, offset, count);
            _offset += count;
        }

        public IHash Hash => _hash;
    }
}
