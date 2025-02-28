using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2 {
    public partial class Client {
        /// <summary>
        /// Configures or modifies the access control list (ACL) for a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.PutBucketAclRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutBucketAclResult" />The result instance.</returns>
        public async Task<Models.PutBucketAclResult> PutBucketAclAsync(
            Models.PutBucketAclRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Acl, "request.Acl");

            var input = new OperationInput {
                OperationName = "PutBucketAcl",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutBucketAclResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.PutBucketAclResult)result;
        }

        /// <summary>
        /// Queries the access control list (ACL) of a bucket. Only the owner of a bucket can query the ACL of the bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.GetBucketAclRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetBucketAclResult" />The result instance.</returns>
        public async Task<Models.GetBucketAclResult> GetBucketAclAsync(
            Models.GetBucketAclRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");

            var input = new OperationInput {
                OperationName = "GetBucketAcl",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = request.Bucket
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetBucketAclResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerXmlBody);

            return (Models.GetBucketAclResult)result;
        }
    }
}