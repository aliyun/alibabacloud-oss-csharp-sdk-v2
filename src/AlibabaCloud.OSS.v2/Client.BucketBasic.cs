using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2 {
    public partial class Client {
        /// <summary>
        /// Queries the storage capacity of a bucket and the number of objects that are stored in the bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.GetBucketStatRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetBucketStatResult" />The result instance.</returns>
        public async Task<Models.GetBucketStatResult> GetBucketStatAsync(
            Models.GetBucketStatRequest request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "GetBucketStat",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "stat", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetBucketStatResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.GetBucketStatResult)result;
        }

        /// <summary>
        /// Creates a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.PutBucketRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutBucketResult" />The result instance.</returns>
        public async Task<Models.PutBucketResult> PutBucketAsync(
            Models.PutBucketRequest request,
            OperationOptions?       options           = null,
            CancellationToken       cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "PutBucket",
                Method        = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutBucketResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.PutBucketResult)result;
        }

        /// <summary>
        /// Deletes a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.DeleteBucketRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.DeleteBucketResult" />The result instance.</returns>
        public async Task<Models.DeleteBucketResult> DeleteBucketAsync(
            Models.DeleteBucketRequest request,
            OperationOptions?          options           = null,
            CancellationToken          cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "DeleteBucket",
                Method        = "DELETE",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.DeleteBucketResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.DeleteBucketResult)result;
        }

        /// <summary>
        /// Queries the information about all objects in a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ListObjectsResult" />The result instance.</returns>
        public async Task<Models.ListObjectsResult> ListObjectsAsync(
            Models.ListObjectsRequest request,
            OperationOptions?         options           = null,
            CancellationToken         cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "ListObjects",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "encoding-type", "url" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListObjectsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListObjects);

            return (Models.ListObjectsResult)result;
        }

        /// <summary>
        /// Queries the information about all objects in a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectsV2Request"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.ListObjectsV2Result" />The result instance.</returns>
        public async Task<Models.ListObjectsV2Result> ListObjectsV2Async(
            Models.ListObjectsV2Request request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "ListObjectsV2",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "list-type", "2" },
                    { "encoding-type", "url" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListObjectsV2Result();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListObjectsV2);

            return (Models.ListObjectsV2Result)result;
        }

        /// <summary>
        /// Queries the information about a bucket. Only the owner of a bucket can query the information about the bucket. You can call this operation from an Object Storage Service (OSS) endpoint.
        /// </summary>
        /// <param name="request"><see cref="Models.GetBucketInfoRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetBucketInfoResult" />The result instance.</returns>
        public async Task<Models.GetBucketInfoResult> GetBucketInfoAsync(
            Models.GetBucketInfoRequest request,
            OperationOptions?           options           = null,
            CancellationToken           cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "GetBucketInfo",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "bucketInfo", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetBucketInfoResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.GetBucketInfoResult)result;
        }

        /// <summary>
        /// Queries the region in which a bucket resides. Only the owner of a bucket can query the region in which the bucket resides.
        /// </summary>
        /// <param name="request"><see cref="Models.GetBucketLocationRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetBucketLocationResult" />The result instance.</returns>
        public async Task<Models.GetBucketLocationResult> GetBucketLocationAsync(
            Models.GetBucketLocationRequest request,
            OperationOptions?               options           = null,
            CancellationToken               cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "GetBucketLocation",
                Method        = "GET",
                Parameters = new Dictionary<string, string> {
                    { "location", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetBucketLocationResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.GetBucketLocationResult)result;
        }
    }
}