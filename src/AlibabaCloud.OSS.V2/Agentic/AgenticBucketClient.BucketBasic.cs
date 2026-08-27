using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.Agentic
{
    public sealed partial class AgenticBucketClient
    {
        private const string ContentTypeXml = "application/xml";

        /// <summary>
        /// Creates an agentic bucket.
        /// </summary>
        public async Task<CreateAgenticBucketResult> CreateAgenticBucketAsync(
            CreateAgenticBucketRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput
            {
                OperationName = "CreateAgenticBucket",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", ContentTypeXml }
                },
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new CreateAgenticBucketResult();
            Serde.DeserializeOutput(ref result, ref output);
            return (CreateAgenticBucketResult)result;
        }

        /// <summary>
        /// Deletes an agentic bucket.
        /// </summary>
        public async Task<DeleteAgenticBucketResult> DeleteAgenticBucketAsync(
            DeleteAgenticBucketRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput
            {
                OperationName = "DeleteAgenticBucket",
                Method = "DELETE",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", ContentTypeXml }
                },
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new DeleteAgenticBucketResult();
            Serde.DeserializeOutput(ref result, ref output);
            return (DeleteAgenticBucketResult)result;
        }

        /// <summary>
        /// Queries the information about an agentic bucket.
        /// </summary>
        public async Task<GetAgenticBucketResult> GetAgenticBucketAsync(
            GetAgenticBucketRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput
            {
                OperationName = "GetAgenticBucket",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new GetAgenticBucketResult();
            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeAgenticXmlBody);
            return (GetAgenticBucketResult)result;
        }

        /// <summary>
        /// Lists the agentic buckets that belong to the current account.
        /// </summary>
        public async Task<ListAgenticBucketsResult> ListAgenticBucketsAsync(
            ListAgenticBucketsRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var input = new OperationInput
            {
                OperationName = "ListAgenticBuckets",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" }
                }
            };

            Serde.SerializeInput(request, ref input);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new ListAgenticBucketsResult();
            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListAgenticBuckets);
            return (ListAgenticBucketsResult)result;
        }

        /// <summary>
        /// Configures the status of an agentic bucket.
        /// </summary>
        public async Task<PutAgenticBucketStatusResult> PutAgenticBucketStatusAsync(
            PutAgenticBucketStatusRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.AgenticBucketStatus, "request.AgenticBucketStatus");

            var input = new OperationInput
            {
                OperationName = "PutAgenticBucketStatus",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", ContentTypeXml }
                },
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" },
                    { "status", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new PutAgenticBucketStatusResult();
            Serde.DeserializeOutput(ref result, ref output);
            return (PutAgenticBucketStatusResult)result;
        }

        /// <summary>
        /// Lists the bucket spaces in an agentic bucket.
        /// </summary>
        public async Task<ListBucketSpacesResult> ListBucketSpacesAsync(
            ListBucketSpacesRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput
            {
                OperationName = "ListBucketSpaces",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "agenticBucket", "" },
                    { "bucketSpace", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _client.InvokeOperationAsync(input, options, cancellationToken).ConfigureAwait(false);

            V2.Models.ResultModel result = new ListBucketSpacesResult();
            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListBucketSpaces);
            return (ListBucketSpacesResult)result;
        }
    }
}
