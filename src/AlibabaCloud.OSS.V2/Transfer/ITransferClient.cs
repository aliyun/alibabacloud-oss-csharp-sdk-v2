using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.Transfer
{
    /// <summary>
    /// The subset of data-plane operations that the high-level <see cref="Uploader"/>,
    /// <see cref="Downloader"/> and <see cref="Copier"/> components depend on.
    /// <para>
    /// This interface lets those components stay transparent to the concrete client type.
    /// Neither <see cref="Client"/> nor any future client (for example a client-side encryption
    /// client) implements this interface directly; instead each concrete client is wrapped by its
    /// own adapter (see <see cref="ClientAdapter"/>). To support a new client type, add a new
    /// adapter and a matching constructor overload on the components.
    /// </para>
    /// </summary>
    internal interface ITransferClient
    {
        ClientOptions GetClientOptions();

        Task<PutObjectResult> PutObjectAsync(
            PutObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<GetObjectResult> GetObjectAsync(
            GetObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<HeadObjectResult> HeadObjectAsync(
            HeadObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<CopyObjectResult> CopyObjectAsync(
            CopyObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<GetObjectTaggingResult> GetObjectTaggingAsync(
            GetObjectTaggingRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<InitiateMultipartUploadResult> InitiateMultipartUploadAsync(
            InitiateMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<UploadPartResult> UploadPartAsync(
            UploadPartRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<UploadPartCopyResult> UploadPartCopyAsync(
            UploadPartCopyRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<CompleteMultipartUploadResult> CompleteMultipartUploadAsync(
            CompleteMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<AbortMultipartUploadResult> AbortMultipartUploadAsync(
            AbortMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);

        Task<ListPartsResult> ListPartsAsync(
            ListPartsRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Adapts the SDK's <see cref="Client"/> to <see cref="ITransferClient"/> by forwarding each
    /// call. This keeps <see cref="Client"/> free of any transfer-specific interface.
    /// </summary>
    internal sealed class ClientAdapter : ITransferClient
    {
        private readonly Client _client;

        public ClientAdapter(Client client) => _client = client;

        public ClientOptions GetClientOptions() => _client.GetClientOptions();

        public Task<PutObjectResult> PutObjectAsync(
            PutObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.PutObjectAsync(request, options, cancellationToken);

        public Task<GetObjectResult> GetObjectAsync(
            GetObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.GetObjectAsync(request, options, cancellationToken);

        public Task<HeadObjectResult> HeadObjectAsync(
            HeadObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.HeadObjectAsync(request, options, cancellationToken);

        public Task<CopyObjectResult> CopyObjectAsync(
            CopyObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.CopyObjectAsync(request, options, cancellationToken);

        public Task<GetObjectTaggingResult> GetObjectTaggingAsync(
            GetObjectTaggingRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.GetObjectTaggingAsync(request, options, cancellationToken);

        public Task<InitiateMultipartUploadResult> InitiateMultipartUploadAsync(
            InitiateMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.InitiateMultipartUploadAsync(request, options, cancellationToken);

        public Task<UploadPartResult> UploadPartAsync(
            UploadPartRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.UploadPartAsync(request, options, cancellationToken);

        public Task<UploadPartCopyResult> UploadPartCopyAsync(
            UploadPartCopyRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.UploadPartCopyAsync(request, options, cancellationToken);

        public Task<CompleteMultipartUploadResult> CompleteMultipartUploadAsync(
            CompleteMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.CompleteMultipartUploadAsync(request, options, cancellationToken);

        public Task<AbortMultipartUploadResult> AbortMultipartUploadAsync(
            AbortMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.AbortMultipartUploadAsync(request, options, cancellationToken);

        public Task<ListPartsResult> ListPartsAsync(
            ListPartsRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
            => _client.ListPartsAsync(request, options, cancellationToken);
    }
}
