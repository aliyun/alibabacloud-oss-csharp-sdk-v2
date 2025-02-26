using System;

namespace AlibabaCloud.OSS.v2.Internal {

    /// <summary>
    /// Hash is the common interface implemented by all hash functions.
    /// </summary>
    public interface IHash {

        public void Update(byte[] buffer, int offset, int count);

        public byte[] Final();

        public void Reset();

        public int Size {  get; }

        public int BlockSize { get; }
    }

    public class HashCrc64 : IHash {
        private ulong _init;
        private ulong _crc;

        public HashCrc64(byte[] init) :
            this(BitConverter.ToUInt64(init, 0)) {
        }

        internal HashCrc64(ulong init) {
            _init = init;
            _crc = _init;
            Crc64.InitECMA();
        }

        public int BlockSize => 1;

        public int Size => 8;

        public void Reset() {
            _crc = _init;
        }

        public void Update(byte[] buffer, int offset, int count) {
            _crc = Crc64.Compute(buffer, offset, count, _crc);
        }

        public byte[] Final() {
            return BitConverter.GetBytes(_crc);
        }
    }
}
