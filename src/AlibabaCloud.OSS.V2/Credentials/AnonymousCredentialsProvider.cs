
namespace AlibabaCloud.OSS.V2.Credentials {
    /// <summary>
    /// Access OSS anonymously.
    /// </summary>
    public class AnonymousCredentialsProvider : ICredentialsProvider {
        private readonly Credentials _credentials;
        /// <summary>
        /// Creates an instance of <see cref="AnonymousCredentialsProvider"/>
        /// </summary>
        public AnonymousCredentialsProvider() {
            _credentials = new("", "");
        }

        /// <inheritdoc/>
        public Credentials GetCredentials() {
            return _credentials;
        }
    }
}
