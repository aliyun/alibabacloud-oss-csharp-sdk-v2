using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2 {
    public partial class Client {
        /// <summary>
        /// Initiates a multipart upload task.
        /// </summary>
        /// <param name="request"><see cref="Models.InitiateMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.InitiateMultipartUploadResult" />The result instance.</returns>
        public async Task<Models.InitiateMultipartUploadResult> InitiateMultipartUploadAsync(
            Models.InitiateMultipartUploadRequest request,
            OperationOptions?                     options           = null,
            CancellationToken                     cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "InitiateMultipartUpload",
                Method        = "POST",
                Parameters = new Dictionary<string, string> {
                    { "uploads", "" },
                    {"encoding-type", "url"}
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddMetadata, Serde.AddContentMd5, AddContentTypeEx);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.InitiateMultipartUploadResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeInitiateMultipartUpload);

            return (Models.InitiateMultipartUploadResult)result;
        }

        /// <summary>
        /// You can call this operation to upload an object by part based on the object name and the upload ID that you specify.
        /// </summary>
        /// <param name="request"><see cref="Models.UploadPartRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.UploadPartResult" />The result instance.</returns>
        public async Task<Models.UploadPartResult> UploadPartAsync(
            Models.UploadPartRequest request,
            OperationOptions?        options           = null,
            CancellationToken        cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.PartNumber, "request.PartNumber");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "UploadPart",
                Method        = "PUT",
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, AddCrcChecker, AddProgress);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.UploadPartResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.UploadPartResult)result;
        }

        /// <summary>
        /// You can call this operation to complete the multipart upload task of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.CompleteMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.CompleteMultipartUploadResult" />The result instance.</returns>
        public async Task<Models.CompleteMultipartUploadResult> CompleteMultipartUploadAsync(
            Models.CompleteMultipartUploadRequest request,
            OperationOptions?                     options           = null,
            CancellationToken                     cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "CompleteMultipartUpload",
                Method        = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    {"encoding-type", "url"}
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.CompleteMultipartUploadResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeCompleteMultipartUpload);

            return (Models.CompleteMultipartUploadResult)result;
        }

        /// <summary>
        /// You can call this operation to copy data from an existing object to upload a part by adding a x-oss-copy-request header to UploadPart.
        /// </summary>
        /// <param name="request"><see cref="Models.UploadPartCopyRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.UploadPartCopyResult" />The result instance.</returns>
        public async Task<Models.UploadPartCopyResult> UploadPartCopyAsync(
            Models.UploadPartCopyRequest request,
            OperationOptions?            options           = null,
            CancellationToken            cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.SourceKey, "request.SourceKey");
            Ensure.NotNull(request.PartNumber, "request.PartNumber");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "UploadPartCopy",
                Method        = "PUT",
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddCopySource);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.UploadPartCopyResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeUploadPartCopy);

            return (Models.UploadPartCopyResult)result;
        }

        /// <summary>
        /// You can call this operation to cancel a multipart upload task and delete the parts that are uploaded by the multipart upload task.
        /// </summary>
        /// <param name="request"><see cref="Models.AbortMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.AbortMultipartUploadResult" />The result instance.</returns>
        public async Task<Models.AbortMultipartUploadResult> AbortMultipartUploadAsync(
            Models.AbortMultipartUploadRequest request,
            OperationOptions?                  options           = null,
            CancellationToken                  cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "AbortMultipartUpload",
                Method        = "DELETE",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.AbortMultipartUploadResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.AbortMultipartUploadResult)result;
        }

        /// <summary>
        /// You can call this operation to list all ongoing multipart upload tasks.
        /// </summary>
        /// <param name="request"><see cref="Models.ListMultipartUploadsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ListMultipartUploadsResult" />The result instance.</returns>
        public async Task<Models.ListMultipartUploadsResult> ListMultipartUploadsAsync(
            Models.ListMultipartUploadsRequest request,
            OperationOptions?                  options           = null,
            CancellationToken                  cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "ListMultipartUploads",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "uploads", "" },
                    { "encoding-type", "url" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListMultipartUploadsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListMultipartUploads);

            return (Models.ListMultipartUploadsResult)result;
        }

        /// <summary>
        /// You can call this operation to list all parts that are uploaded by using a specified upload ID.
        /// </summary>
        /// <param name="request"><see cref="Models.ListPartsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ListPartsResult" />The result instance.</returns>
        public async Task<Models.ListPartsResult> ListPartsAsync(
            Models.ListPartsRequest request,
            OperationOptions?       options           = null,
            CancellationToken       cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "ListParts",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "encoding-type", "url" }
                },
                Bucket        = request.Bucket,
                Key           = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListPartsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListParts);

            return (Models.ListPartsResult)result;
        }

        private void AddContentTypeEx(ref Models.RequestModel request, ref OperationInput input) {
            if (request is Models.InitiateMultipartUploadRequest req) {
                if (req.DisableAutoDetectMimeType) {
                    return;
                }
            }
            AddContentType(ref request, ref input);
        }
    }
}