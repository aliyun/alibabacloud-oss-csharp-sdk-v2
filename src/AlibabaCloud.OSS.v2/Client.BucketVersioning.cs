using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2 {
    public partial class Client {
        /// <summary>
        /// Configures the versioning state for a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.PutBucketVersioningRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutBucketVersioningResult" />The result instance.</returns>
        public async Task<Models.PutBucketVersioningResult> PutBucketVersioningAsync(
            Models.PutBucketVersioningRequest request,
            OperationOptions?                 options           = null,
            CancellationToken                 cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.VersioningConfiguration, "request.VersioningConfiguration");

            var input = new OperationInput {
                OperationName = "PutBucketVersioning",
                Method        = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "versioning", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutBucketVersioningResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.PutBucketVersioningResult)result;
        }

        /// <summary>
        /// Queries the versioning state of a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.GetBucketVersioningRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetBucketVersioningResult" />The result instance.</returns>
        public async Task<Models.GetBucketVersioningResult> GetBucketVersioningAsync(
            Models.GetBucketVersioningRequest request,
            OperationOptions?                 options           = null,
            CancellationToken                 cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "GetBucketVersioning",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "versioning", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetBucketVersioningResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeGetBucketVersioning);

            return (Models.GetBucketVersioningResult)result;
        }

        /// <summary>
        /// Queries the information about the versions of all objects in a bucket, including the delete markers.
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectVersionsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ListObjectVersionsResult" />The result instance.</returns>
        public async Task<Models.ListObjectVersionsResult> ListObjectVersionsAsync(
            Models.ListObjectVersionsRequest request,
            OperationOptions?                options           = null,
            CancellationToken                cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "ListObjectVersions",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "versions", "" },
                    { "encoding-type", "url" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListObjectVersionsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListObjectVersions);

            return (Models.ListObjectVersionsResult)result;
        }
    }
}