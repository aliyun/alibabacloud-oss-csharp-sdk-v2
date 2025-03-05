using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2
{
    public partial class Client
    {
        /// <summary>
        /// Queries the endpoints of all supported regions or the endpoints of a specific region.
        /// </summary>
        /// <param name="request"><see cref="Models.DescribeRegionsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.DescribeRegionsResult" />The result instance.</returns>
        public async Task<Models.DescribeRegionsResult> DescribeRegionsAsync(
            Models.DescribeRegionsRequest request,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var input = new OperationInput
            {
                OperationName = "DescribeRegions",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "regions", "" }
                },
                OperationMetadata = {
                    ["sub-resource"] = new List<string>() { "regions" }
                }
            };

            Serde.SerializeInput(request, ref input);

            var output = await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);

            Models.ResultModel result = new Models.DescribeRegionsResult();

            Serde.DeserializeOutput(ref result, ref output, Serde.DeserializerAnyBody);

            return (Models.DescribeRegionsResult)result;
        }
    }
}
