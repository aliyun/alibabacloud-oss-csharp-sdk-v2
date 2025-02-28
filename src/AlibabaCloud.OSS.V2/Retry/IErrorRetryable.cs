
using System;

namespace AlibabaCloud.OSS.V2.Retry {
    public interface IErrorRetryable {
        /// <summary>
        /// Check whether the error is retryable.
        /// </summary>
        /// <param name="error">the error meets</param>
        /// <returns>True if the error is retryable.</returns>
        public bool IsErrorRetryable(Exception error);
    }
}
