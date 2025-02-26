using System;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.v2 {
    public partial class Client : IDisposable {
        private readonly Internal.ClientImpl _clientImpl;

        public Client(
            Configuration                  config,
            params Action<ClientOptions>[] optFns
        ) {
            _clientImpl = new(config, optFns);
        }

        /// <summary>
        /// The generic operations call.
        /// </summary>
        /// <param name="input"><see cref="OperationInput"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="OperationOutput" />The result instance.</returns>
        /// <returns></returns>
        public async Task<OperationOutput> InvokeOperation(
            OperationInput    input,
            OperationOptions? options           = null,
            CancellationToken cancellationToken = default
        ) {
            return await _clientImpl.ExecuteAsync(input, options, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose() {
            _clientImpl.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}