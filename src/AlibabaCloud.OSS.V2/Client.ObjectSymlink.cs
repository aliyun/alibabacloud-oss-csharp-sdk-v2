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
        /// You can create a symbolic link for a target object.
        /// </summary>
        /// <param name="request"><see cref="Models.PutSymlinkRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutSymlinkResult" />The result instance.</returns>
        public async Task<Models.PutSymlinkResult> PutSymlinkAsync(
            Models.PutSymlinkRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.SymlinkTarget, "request.SymlinkTarget");

            var input = new OperationInput
            {
                OperationName = "PutSymlink",
                Method = "PUT",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" }
                },
                Parameters = new Dictionary<string, string> {
                    { "symlink", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddContentMd5);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.PutSymlinkResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.PutSymlinkResult)result;
        }

        /// <summary>
        /// You can call this operation to query a symbolic link of an object.
        /// </summary>
        /// <param name="request"><see cref="Models.GetSymlinkRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetSymlinkResult" />The result instance.</returns>
        public async Task<Models.GetSymlinkResult> GetSymlinkAsync(
            Models.GetSymlinkRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput
            {
                OperationName = "GetSymlink",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "symlink", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.GetSymlinkResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.GetSymlinkResult)result;
        }
    }
}
