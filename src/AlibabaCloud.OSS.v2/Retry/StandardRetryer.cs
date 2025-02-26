
using System;

namespace AlibabaCloud.OSS.v2.Retry {
    public class StandardRetryer : IRetryer
    {
        private int _maxAttempts;
        private IErrorRetryable[] _errorRetryables;
        private IBackoffDelayer _backoffDelayer;

        private readonly IErrorRetryable[] defaultErrorRetryables = {
            new HttpStatusCodeRetryable(),
            new ServiceErrorCodeRetryable(),
            new ClientExceptionRetryable(),
        };

        public StandardRetryer(
            int? maxAttempts = null, 
            TimeSpan? maxBackoff = null, 
            TimeSpan? baseDelay = null,
            IErrorRetryable[]? errorRetryables = null,
            IBackoffDelayer? backoffDelayer = null) 
        {
            _maxAttempts = maxAttempts ?? Defaults.MaxAttpempts;
            _backoffDelayer = backoffDelayer?? new FullJitterBackoff(
                maxBackoff ?? Defaults.MaxBackOff, 
                baseDelay ?? Defaults.BaseDelay);
            _errorRetryables = errorRetryables ?? defaultErrorRetryables;
        }

        public bool IsErrorRetryable(Exception error)
        {
            foreach (var retryable in _errorRetryables) {
                if (retryable.IsErrorRetryable(error)) {
                    return true;
                }
            }
            return false;
        }

        public int MaxAttempts()
        {
            return _maxAttempts;
        }

        public TimeSpan RetryDelay(int attempt, Exception error)
        {
            return _backoffDelayer.BackofDelay(attempt, error);
        }
    }
}
