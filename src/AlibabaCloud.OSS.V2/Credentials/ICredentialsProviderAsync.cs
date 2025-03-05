
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Credentials
{
    /// <summary>
    /// ICredentialsProvider Interface
    /// </summary>
    public interface ICredentialsProviderAsync
    {
        /// <summary>
        /// Gets an instance of <see cref="Credentials"/>  async.
        /// </summary>
        /// <returns><see cref="Credentials"/>a Credentials instance</returns>
        Task<Credentials> GetCredentialsAsync(CancellationToken cancellationToken);
    }
}
