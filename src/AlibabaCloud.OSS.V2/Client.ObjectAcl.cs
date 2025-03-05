using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2
{
    public partial class Client
    {
        /// <summary>
        /// You can call this operation to modify the ACL of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.PutObjectAclRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutObjectAclResult" />The result instance.</returns>
        public async Task<Models.PutObjectAclResult> PutObjectAclAsync(
            Models.PutObjectAclRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.Acl, "request.Acl");

            var input = new OperationInput
            {
                OperationName = "PutObjectAcl",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutObjectAclResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.PutObjectAclResult)result;
        }

        /// <summary>
        /// You can call this operation to query the ACL of an object in a bucket.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectAclRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectAclResult" />The result instance.</returns>
        public async Task<Models.GetObjectAclResult> GetObjectAclAsync(
            Models.GetObjectAclRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput
            {
                OperationName = "GetObjectAcl",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetObjectAclResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.GetObjectAclResult)result;
        }
    }
}
