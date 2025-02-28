
namespace AlibabaCloud.OSS.V2.Credentials {
    /// <summary>
    /// ICredentialsProvider Interface
    /// </summary>
    public interface ICredentialsProvider {
        /// <summary>
        /// Gets an instance of <see cref="Credentials"/>
        /// </summary>
        /// <returns><see cref="Credentials"/>a Credentials instance</returns>
        Credentials GetCredentials();
    }
}
