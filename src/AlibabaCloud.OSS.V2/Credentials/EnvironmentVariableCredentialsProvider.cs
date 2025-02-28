using System;

namespace AlibabaCloud.OSS.V2.Credentials {
    /// <summary>
    /// Obtaining credentials from environment variables.
    /// OSS_ACCESS_KEY_ID
    /// OSS_ACCESS_KEY_SECRET
    /// OSS_SESSION_TOKEN(Optional)
    /// </summary>
    public class EnvironmentVariableCredentialsProvider : ICredentialsProvider {
        private readonly Credentials _credentials;
        /// <summary>
        /// Creates an instance of <see cref="EnvironmentVariableCredentialsProvider"/>
        /// </summary>
        public EnvironmentVariableCredentialsProvider() {
            var ak = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_ID");
            var sk = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_SECRET");
            if (string.IsNullOrEmpty(ak) || string.IsNullOrEmpty(sk)) {
                throw new ArgumentException("Credentials is null or empty");
            }
            var token = Environment.GetEnvironmentVariable("OSS_SESSION_TOKEN");

            _credentials = new Credentials(ak, sk, token ?? "");
        }

        /// <inheritdoc/>
        public Credentials GetCredentials() {
            return _credentials;
        }
    }
}
