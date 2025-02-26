
using AlibabaCloud.OSS.v2.Transform;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.v2 {
    public partial class Client {
        public async Task<Models.ListBucketsResult> ListBucketsAsync(
            Models.ListBucketsRequest request,
            OperationOptions? options = null, 
            CancellationToken cancellationToken = default) {

            var input = new OperationInput {
                OperationName = "ListBuckets",
                Method = "GET",
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.ListBucketsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializeListBuckets);

            return (Models.ListBucketsResult)result;
        }
    }
}
