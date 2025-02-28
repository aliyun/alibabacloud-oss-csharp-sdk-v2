using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#if NETCOREAPP
using System.Net.Security;
#endif

namespace AlibabaCloud.OSS.V2.Transport {
    /// <summary>
    /// An implementation that uses <see cref="HttpClient"/> as the transport.
    /// </summary>
    public class HttpTransport : IDisposable {
        /// <summary>
        /// A shared instance of <see cref="HttpTransport"/> with default parameters.
        /// </summary>
        public static readonly HttpTransport Shared = new HttpTransport();

        // The transport's private HttpClient is internal because it is used by tests.
        internal HttpClient Client { get; }
        internal bool _disposeClient = true;

        /// <summary>
        /// Creates a new <see cref="HttpTransport"/> instance using default configuration.
        /// </summary>
        public HttpTransport() : this(CreateDefaultClient()) { }

        /// <summary>
        /// Creates a new instance of <see cref="HttpTransport"/> using the provided client instance.
        /// </summary>
        /// <param name="messageHandler">The instance of <see cref="HttpMessageHandler"/> to use.</param>
        public HttpTransport(HttpMessageHandler messageHandler) {
            Client = new HttpClient(messageHandler) ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpTransport"/> using the provided client instance.
        /// </summary>
        /// <param name="client">The instance of <see cref="HttpClient"/> to use.</param>
        /// <param name="disposeClient">true if the inner client should be disposed of by Dispose(), false if you intend
        /// to reuse the inner client.</param>
        public HttpTransport(HttpClient client, bool disposeClient = true) {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            _disposeClient = disposeClient;
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="completionOption">When the operation should complete (as soon as a response is available or after
        /// reading the whole response content)</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// </summary>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) {
            return Client.SendAsync(request, completionOption, cancellationToken);
        }

        /// <summary>
        /// Disposes the underlying <see cref="HttpClient"/>.
        /// </summary>
        public void Dispose() {
            if (this != Shared) {
                if (_disposeClient) {
                    Client.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }

        static HttpClient CreateDefaultClient(HttpTransportOptions? options = null) {
            return CreateCustomClient();
        }

        public static HttpClient CreateCustomClient(HttpTransportOptions? options = null) {
            var httpMessageHandler = CreateDefaultHandler(options);
            return new HttpClient(httpMessageHandler) {
                // Timeouts are handled by the pipeline
                Timeout = Timeout.InfiniteTimeSpan,
            };
        }

#if NETCOREAPP
        private static HttpMessageHandler CreateDefaultHandler(HttpTransportOptions? options = null)
        {
            var opt = options ?? new HttpTransportOptions();
            var handler = new SocketsHttpHandler {
                ConnectTimeout = opt.ConnectTimeout.GetValueOrDefault(HttpTransportOptions.DEFAULT_CONNECT_TIMEOUT),
                Expect100ContinueTimeout = opt.ExpectContinueTimeout.GetValueOrDefault(HttpTransportOptions.DEFAULT_EXPECT_CONTINUE_TIMEOUT),
                PooledConnectionIdleTimeout = opt.IdleConnectionTimeout.GetValueOrDefault(HttpTransportOptions.DEFAULT_IDLE_CONNECTION_TIMEOUT),
                KeepAlivePingTimeout = opt.KeepAliveTimeout.GetValueOrDefault(HttpTransportOptions.DEFAULT_KEEP_ALIVE_TIMEOUT),
                MaxConnectionsPerServer = opt.MaxConnections.GetValueOrDefault(HttpTransportOptions.DEFAULT_MAX_CONNECTIONS),
                AllowAutoRedirect = opt.EnabledRedirect.GetValueOrDefault(false),
                Proxy = opt.HttpProxy
            };
            if (opt.InsecureSkipVerify.GetValueOrDefault(false))
            {
                handler.SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = delegate { return true; },
                };
            }
            return handler;
        }
#else
        private static HttpMessageHandler CreateDefaultHandler(HttpTransportOptions? options = null) {
            var opt = options ?? new HttpTransportOptions();
            var handler = new HttpClientHandler {
                MaxConnectionsPerServer = opt.MaxConnections.GetValueOrDefault(HttpTransportOptions.DEFAULT_MAX_CONNECTIONS),
                AllowAutoRedirect = opt.EnabledRedirect.GetValueOrDefault(false),
                Proxy = opt.HttpProxy
            };
            if (opt.InsecureSkipVerify.GetValueOrDefault(false)) {
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };
            }
            return handler;
        }
#endif

    }
}
