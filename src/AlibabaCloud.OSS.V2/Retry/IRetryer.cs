
using System;

namespace AlibabaCloud.OSS.V2.Retry {
    /// <summary>
    /// The interface for Retryer.
    /// </summary>
    public interface IRetryer {
        /// <summary>
        /// Check whether the error is retryable.
        /// </summary>
        /// <param name="error">the error meets</param>
        /// <returns>True if the error is retryable.</returns>
        public bool IsErrorRetryable(Exception error);

        /// <summary>
        /// Retrieve max attempts.
        /// </summary>
        /// <returns>max attempts</returns>
        public int MaxAttempts();

        /// <summary>
        /// Returns the delay that should be used before retrying the attempt.
        /// </summary>
        /// <param name="attempt">current retry attempt</param>
        /// <param name="error">the error meets</param>
        /// <returns>delay duration in TimeSpan</returns>
        public TimeSpan RetryDelay(int attempt, Exception error);
    }
}
