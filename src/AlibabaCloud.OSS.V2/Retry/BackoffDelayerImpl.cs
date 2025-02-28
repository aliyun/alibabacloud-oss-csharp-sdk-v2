
using System;

namespace AlibabaCloud.OSS.V2.Retry {
    /// <summary>
    /// FixedDelayBackoff implements fixed backoff.
    /// </summary>
    public class FixedDelayBackoff : IBackoffDelayer {
        private TimeSpan _backoff;

        public FixedDelayBackoff(TimeSpan backoff) {
            _backoff = backoff;
        }

        public TimeSpan BackofDelay(int attempt, Exception error) {
            return _backoff;
        }
    }

    /// <summary>
    /// FullJitterBackoff implements capped exponential backoff with jitter.
    /// [0.0, 1.0) * min(2 ^ attempts * baseDealy, maxBackoff)
    /// </summary>
    public class FullJitterBackoff : IBackoffDelayer {
        private TimeSpan _baseDelay;
        private TimeSpan _maxBackoff;
        private int _attemptCelling;

        public FullJitterBackoff(TimeSpan baseDelay, TimeSpan maxBackoff) {
            _baseDelay = baseDelay;
            _maxBackoff = maxBackoff;
            _attemptCelling = (int)Math.Log((double)(long.MaxValue) / baseDelay.TotalSeconds, 2);
        }

        public TimeSpan BackofDelay(int attempt, Exception error) {
            attempt = Math.Min(attempt, _attemptCelling);
            var delayS = Math.Min(_baseDelay.TotalSeconds * (1 << attempt), _maxBackoff.TotalSeconds);
            var rand = new Random().NextDouble();
            return TimeSpan.FromSeconds(delayS * rand);
        }
    }

    /// <summary>
    /// EqualJitterBackoff implements capped exponential backoff with jitter.
    /// ceil = min(2 ^ attempts * baseDealy, maxBackoff)
    /// ceil/2 + [0.0, 1.0) *(ceil/2 + 1)
    /// </summary>
    public class EqualJitterBackoff : IBackoffDelayer {
        private TimeSpan _baseDelay;
        private TimeSpan _maxBackoff;
        private int _attemptCelling;

        /// <summary>
        /// Creates EqualJitterBackoff
        /// </summary>
        /// <param name="baseDelay">the base delay duration</param>
        /// <param name="maxBackoff">the max duration</param>
        public EqualJitterBackoff(TimeSpan baseDelay, TimeSpan maxBackoff) {
            _baseDelay = baseDelay;
            _maxBackoff = maxBackoff;
            _attemptCelling = (int)Math.Log((double)(long.MaxValue) / baseDelay.TotalSeconds, 2);
        }

        public TimeSpan BackofDelay(int attempt, Exception error) {
            attempt = Math.Min(attempt, _attemptCelling);
            var delayS = Math.Min(_baseDelay.TotalSeconds * (1 << attempt), _maxBackoff.TotalSeconds);
            var halfS = delayS / 2;
            var rand = new Random().NextDouble();
            return TimeSpan.FromSeconds(halfS + (halfS + 1) * rand);
        }
    }
}