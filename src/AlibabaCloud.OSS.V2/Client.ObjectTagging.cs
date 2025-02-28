using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2 {
    public partial class Client {
        /// <summary>
        /// You can call this operation to add tags to or modify the tags of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.PutObjectTaggingRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutObjectTaggingResult" />The result instance.</returns>
        public async Task<Models.PutObjectTaggingResult> PutObjectTaggingAsync(
            Models.PutObjectTaggingRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.Tagging, "request.Tagging");

            var input = new OperationInput {
                OperationName = "PutObjectTagging",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "tagging", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutObjectTaggingResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.PutObjectTaggingResult)result;
        }

        /// <summary>
        /// You can call this operation to query the tags of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectTaggingRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectTaggingResult" />The result instance.</returns>
        public async Task<Models.GetObjectTaggingResult> GetObjectTaggingAsync(
            Models.GetObjectTaggingRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "GetObjectTagging",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "tagging", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetObjectTaggingResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.GetObjectTaggingResult)result;
        }

        /// <summary>
        /// You can call this operation to delete the tags of a specified object.
        /// </summary>
        /// <param name="request"><see cref="Models.DeleteObjectTaggingRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.DeleteObjectTaggingResult" />The result instance.</returns>
        public async Task<Models.DeleteObjectTaggingResult> DeleteObjectTaggingAsync(
            Models.DeleteObjectTaggingRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "DeleteObjectTagging",
                Method = "DELETE",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "tagging", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.DeleteObjectTaggingResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.DeleteObjectTaggingResult)result;
        }
    }
}