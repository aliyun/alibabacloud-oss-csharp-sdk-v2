using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.v2.Internal;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2 {
    public partial class Client {
        /// <summary>
        /// You can call this operation to upload an object.
        /// </summary>
        /// <param name="request"><see cref="Models.PutObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutObjectResult" />The result instance.</returns>
        public async Task<Models.PutObjectResult> PutObjectAsync(
            Models.PutObjectRequest request,
            OperationOptions?       options           = null,
            CancellationToken       cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "PutObject",
                Method        = "PUT",
                Bucket        = request.Bucket,
                Key           = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddMetadata, AddContentType, AddCrcChecker, AddProgress);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutObjectResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.PutObjectResult)result;
        }

        /// <summary>
        /// Copies objects within a bucket or between buckets in the same region.
        /// </summary>
        /// <param name="request"><see cref="Models.CopyObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.CopyObjectResult" />The result instance.</returns>
        public async Task<Models.CopyObjectResult> CopyObjectAsync(
            Models.CopyObjectRequest request,
            OperationOptions?        options           = null,
            CancellationToken        cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.SourceKey, "request.SourceKey");

            var input = new OperationInput {
                OperationName = "CopyObject",
                Method        = "PUT",
                Bucket        = request.Bucket,
                Key           = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddCopySource, Serde.AddMetadata);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.CopyObjectResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeCopyObject);

            return (Models.CopyObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to query an object as streams.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectResult" />The result instance.</returns>
        public async Task<Models.GetObjectResult> GetObjectAsync(
            Models.GetObjectRequest request,
            OperationOptions?       options           = null,
            CancellationToken       cancellationToken = default
        ) {
            return await GetObjectAsync(request, HttpCompletionOption.ResponseHeadersRead, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// You can call this operation to query an object.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectRequest"/>The request parameter to send.</param>
        /// <param name="completionOption">Indicates if the operations should be considered completed either as soon as a response is available, or after reading the entire response message including the content.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectResult" />The result instance.</returns>
        public async Task<Models.GetObjectResult> GetObjectAsync(
            Models.GetObjectRequest request,
            HttpCompletionOption completionOption,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "GetObject",
                Method = "GET",
                Bucket = request.Bucket,
                Key = request.Key,
                OperationMetadata = new Dictionary<string, object> {
                    { "http-completion-option", completionOption}
                }
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetObjectResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.GetObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to upload an object by appending the object to an existing object.
        /// </summary>
        /// <param name="request"><see cref="Models.AppendObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.AppendObjectResult" />The result instance.</returns>
        public async Task<Models.AppendObjectResult> AppendObjectAsync(
            Models.AppendObjectRequest request,
            OperationOptions?          options           = null,
            CancellationToken          cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.Position, "request.Position");

            var input = new OperationInput {
                OperationName = "AppendObject",
                Method        = "POST",
                Parameters = new Dictionary<string, string> {
                    { "append", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input,
                Serde.AddMetadata,
                AddContentType,
                AddCrcCheckerNoRetry,
                AddProgress
            );

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.AppendObjectResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.AppendObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to delete an object.
        /// </summary>
        /// <param name="request"><see cref="Models.DeleteObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.DeleteObjectResult" />The result instance.</returns>
        public async Task<Models.DeleteObjectResult> DeleteObjectAsync(
            Models.DeleteObjectRequest request,
            OperationOptions?          options           = null,
            CancellationToken          cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "DeleteObject",
                Method        = "DELETE",
                Bucket        = request.Bucket,
                Key           = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.DeleteObjectResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.DeleteObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to query the metadata of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.HeadObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.HeadObjectResult" />The result instance.</returns>
        public async Task<Models.HeadObjectResult> HeadObjectAsync(
            Models.HeadObjectRequest request,
            OperationOptions?        options           = null,
            CancellationToken        cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "HeadObject",
                Method        = "HEAD",
                Bucket        = request.Bucket,
                Key           = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.HeadObjectResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.HeadObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to query the metadata of an object, including ETag, Size, and LastModified. The content of the object is not returned.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectMetaRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectMetaResult" />The result instance.</returns>
        public async Task<Models.GetObjectMetaResult> GetObjectMetaAsync(
            Models.GetObjectMetaRequest request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "GetObjectMeta",
                Method        = "HEAD",
                Parameters = new Dictionary<string, string> {
                    { "objectMeta", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetObjectMetaResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.GetObjectMetaResult)result;
        }

        /// <summary>
        /// You can call this operation to restore objects of the Archive and Cold Archive storage classes.
        /// </summary>
        /// <param name="request"><see cref="Models.RestoreObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.RestoreObjectResult" />The result instance.</returns>
        public async Task<Models.RestoreObjectResult> RestoreObjectAsync(
            Models.RestoreObjectRequest request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "RestoreObject",
                Method        = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "restore", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.RestoreObjectResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.RestoreObjectResult)result;
        }

        /// <summary>
        /// You can call this operation to clean an object restored from Archive or Cold Archive state. After that, the restored object returns to the frozen state.
        /// </summary>
        /// <param name="request"><see cref="Models.CleanRestoredObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.CleanRestoredObjectResult" />The result instance.</returns>
        public async Task<Models.CleanRestoredObjectResult> CleanRestoredObjectAsync(
            Models.CleanRestoredObjectRequest request,
            OperationOptions?                 options           = null,
            CancellationToken                 cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "CleanRestoredObject",
                Method        = "POST",
                Parameters = new Dictionary<string, string> {
                    { "cleanRestoredObject", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key,
                OperationMetadata = {
                    ["sub-resource"] = new List<string>() { "cleanRestoredObject" }
                }
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.CleanRestoredObjectResult();

            Serde.DeserializeOutput(ref result, ref output);

            return (Models.CleanRestoredObjectResult)result;
        }

        /// <summary>
        /// Applies process on the specified image file.
        /// </summary>
        /// <param name="request"><see cref="Models.ProcessObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ProcessObjectResult" />The result instance.</returns>
        public async Task<Models.ProcessObjectResult> ProcessObjectAsync(
            Models.ProcessObjectRequest request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.Process, "request.Process");

            var input = new OperationInput {
                OperationName = "ProcessObject",
                Method        = "POST",
                Parameters = new Dictionary<string, string> {
                    { "x-oss-process", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddProcessAction, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ProcessObjectResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.ProcessObjectResult)result;
        }

        /// <summary>
        /// Applies async process on the specified image file.
        /// </summary>
        /// <param name="request"><see cref="Models.AsyncProcessObjectRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.AsyncProcessObjectResult" />The result instance.</returns>
        public async Task<Models.AsyncProcessObjectResult> AsyncProcessObjectAsync(
            Models.AsyncProcessObjectRequest request,
            OperationOptions?                options           = null,
            CancellationToken                cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.Process, "request.Process");

            var input = new OperationInput {
                OperationName = "AsyncProcessObject",
                Method        = "POST",
                Parameters = new Dictionary<string, string> {
                    { "x-oss-async-process", "" }
                },
                Bucket = request.Bucket,
                Key    = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddProcessAction, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.AsyncProcessObjectResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.AsyncProcessObjectResult)result;
        }

        /// <summary>
        /// Deletes multiple objects from a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.DeleteMultipleObjectsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.DeleteMultipleObjectsResult" />The result instance.</returns>
        public async Task<Models.DeleteMultipleObjectsResult> DeleteMultipleObjects(
            Models.DeleteMultipleObjectsRequest request,
            OperationOptions?                   options           = null,
            CancellationToken                   cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Objects, "request.Objects");

            var input = new OperationInput {
                OperationName = "DeleteMultipleObjects",
                Method        = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "delete", "" },
                    { "encoding-type", "url" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.SerializeDeleteMultipleObjects, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.DeleteMultipleObjectsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeDeleteMultipleObjects);

            return (Models.DeleteMultipleObjectsResult)result;
        }

        private void AddContentType(ref Models.RequestModel request, ref OperationInput input) {
            if (!_clientImpl.Options.FeatureFlags.HasFlag(FeatureFlagsType.AutoDetectMimeType)) {
                return;
            }
            Serde.AddContentType(ref request, ref input);
        }

        private void AddCrcChecker(ref Models.RequestModel request, ref OperationInput input) {
            if (!_clientImpl.Options.FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckUpload)) {
                return;
            }

            var tracker = new WriteOnlyHashStream(new HashCrc64(BitConverter.GetBytes((ulong)0)));
            Action<ResponseMessage> handler = x => {
                if (x.Headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc)) {
                    var val = tracker.Hash.Final();
                    var ccrc = Convert.ToString(BitConverter.ToUInt64(val, 0), CultureInfo.InvariantCulture);
                    if (!string.Equals(ccrc, scrc)) {
                        x.Headers.TryGetValue("x-oss-request-id", out var requestId);
                        throw new InconsistentException(ccrc, scrc, requestId?? "");
                    }
                }
            };

            input.AddStreamTracker(tracker);
            input.AddResponseHandler(handler);
        }

        private void AddCrcCheckerNoRetry(ref Models.RequestModel request, ref OperationInput input) {
            if (!_clientImpl.Options.FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckUpload)) {
                return;
            }

            ulong crcInit = 0;
            if (request is Models.AppendObjectRequest req) {
                // ignore crc check
                if (req.InitHashCrc64 == null) {
                    return;
                }
                crcInit = Convert.ToUInt64(req.InitHashCrc64);
            }

            var tracker = new WriteOnlyHashStream(new HashCrc64(BitConverter.GetBytes(crcInit)));
            Action<ResponseMessage> handler = x => {
                if (x.Headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc)) {
                    var val = tracker.Hash.Final();
                    var ccrc = Convert.ToString(BitConverter.ToUInt64(val, 0), CultureInfo.InvariantCulture);
                    if (!string.Equals(ccrc, scrc)) {
                        x.Headers.TryGetValue("x-oss-request-id", out var requestId);
                        throw new NoRetryableInconsistentException(ccrc, scrc, requestId ?? "");
                    }
                }
            };

            input.AddStreamTracker(tracker);
            input.AddResponseHandler(handler);
        }

        private void AddProgress(ref Models.RequestModel request, ref OperationInput input) {
            ProgressFunc? func;
            switch (request) {
                case Models.PutObjectRequest req1:
                    if (req1.ProgressFn == null) {
                        return;
                    }
                    func = req1.ProgressFn;
                    break;
                case Models.AppendObjectRequest req2:
                    if (req2.ProgressFn == null) {
                        return;
                    }
                    func = req2.ProgressFn;
                    break;
                case Models.UploadPartRequest req3:
                    if (req3.ProgressFn == null) {
                        return;
                    }
                    func = req3.ProgressFn;
                    break;
                default:
                    return;
            }
            long total = 0;
            if (input.Body != null) {
                total = input.Body.Length;
            }
            var tracker = new ProgressStream(func, total);
            input.AddStreamTracker(tracker);
        }

        private void AddDownloadCrcChecker(ref Models.RequestModel request, ref OperationInput input) {
            if (!_clientImpl.Options.FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckDownload)) {
                return;
            }

            Action<ResponseMessage> handler = x => {
                if (x.Content == null || !x.Content.CanSeek) {
                    return;
                }

                //skip Partial Content
                if (x.StatusCode == 206) {
                    return;
                }

                if (x.Headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc)) {
                    var ccrc = "0";
                    if (x.Content.Length > 0) {
                        var tracker = new WriteOnlyHashStream(new HashCrc64(0));
                        x.Content.CopyTo(tracker);
                        x.Content.Seek(0, SeekOrigin.Begin);
                        var val = tracker.Hash.Final();
                        ccrc = Convert.ToString(BitConverter.ToUInt64(val, 0), CultureInfo.InvariantCulture);
                    }
                    if (!string.Equals(ccrc, scrc)) {
                        x.Headers.TryGetValue("x-oss-request-id", out var requestId);
                        throw new InconsistentException(ccrc, scrc, requestId ?? "");
                    }
                }
            };
            input.AddResponseHandler(handler);
        }
    }
}