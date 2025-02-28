
using System;

namespace AlibabaCloud.OSS.V2.Retry {
    public class NopRetryer : IRetryer {
        public bool IsErrorRetryable(Exception error) {
            return false;
        }

        public int MaxAttempts() {
            return 1;
        }

        public TimeSpan RetryDelay(int attempt, Exception error) {
            throw new NotImplementedException("not retrying any attempt errors");
        }
    }
}
