
using System;

namespace AlibabaCloud.OSS.V2.Retry {
    public interface IBackoffDelayer
    {
        /// <summary>
        /// Returns the delay that should be used before retrying the attempt.
        /// </summary>
        /// <param name="attempt">current retry attempt</param>
        /// <param name="error">the error meets</param>
        /// <returns>delay duration in TimeSpan</returns>
        public TimeSpan BackofDelay(int attempt, Exception error);
    }
}
