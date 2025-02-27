using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.IntegrationTests {
    public class TimeoutReadStream : WrapperStream {
        private TimeSpan _timeout;
        public TimeoutReadStream(Stream baseStream, TimeSpan timeout) : base(baseStream) {
            _timeout = timeout;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            Thread.Sleep(_timeout);
            var n =  base.Read(buffer, offset, count);
            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            await Task.Delay(_timeout);
            return await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
    }
}
