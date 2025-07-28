using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Internal
{
    internal class InnerOptions
    {
        public string UserAgent { get; set; } = "";
    }

    internal class PresignInnerResult
    {
        public string? Url;
        public string? Method;
        public DateTime? Expiration;
        public Dictionary<string, string>? SignedHeaders;
    }

    internal class ClientImpl : IDisposable
    {
        internal readonly Configuration Config;
        internal readonly ClientOptions Options;
        internal readonly InnerOptions InnerOptions;

        private readonly ExecuteStack _executeStack;
        private readonly OperationOptions _defaultOpOptions = new();
        private readonly OperationOptions _defaultPresignOpOptions = new() { AuthMethod = AuthMethodType.Query };

        public ClientImpl(Configuration config, params Action<ClientOptions>[] optFns)
        {
            // apply config & options
            var opts = ResolveConfig(config);

            foreach (var fn in optFns)
            {
                fn(opts);
            }

            Config = config;
            Options = opts;
            InnerOptions = new InnerOptions()
            {
                UserAgent = ResolveUserAgent(config)
            };

            // build execute stack
            var stack = new ExecuteStack(opts.HttpTransport);

            stack.Push(
                x => new RetryerExecuteMiddleware(x, opts.Retryer),
                "Retryer"
            );

            stack.Push(
                x => new SignerExecuteMiddleware(x, opts.Signer, opts.CredentialsProvider),
                "Signer"
            );

            stack.Push(
                x => new ResponseCheckerExecuteMiddleware(x),
                "ResponseChecker"
            );
            _executeStack = stack;
        }

        public async Task<OperationOutput> ExecuteAsync(
            OperationInput input,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            // verify input
            VerifyOperation(ref input);

            // build execute context;
            var (request, context) = BuildRequestContext(input, options ?? _defaultOpOptions);
            context.ApiCallCancellationToken = cancellationToken;

            // execute and wait result
            try
            {
                var result = await _executeStack.ExecuteAsync(request, context).ConfigureAwait(false);

                return new()
                {
                    StatusCode = result.StatusCode,
                    Status = result.Status,
                    Headers = result.Headers,
                    Body = result.Content,
                    Input = input
                };
            }
            catch (Exception ex)
            {
                throw new OperationException(input.OperationName, ex);
            }
        }

        public PresignInnerResult PresignInner(
            OperationInput input,
            OperationOptions? options = null
        )
        {
            // verify input
            VerifyOperation(ref input);

            // build execute context;
            var (request, context) = BuildRequestContext(input, options ?? _defaultPresignOpOptions);
            var provider = Options.CredentialsProvider;
            var result = new PresignInnerResult();

            if (provider != null &&
                provider is not Credentials.AnonymousCredentialsProvider &&
                context.SigningContext != null)
            {
                var cred = provider.GetCredentials();
                var singer = Options.Signer;
                if (!cred.HasKeys) throw new("Credentials is null or empty");
                context.SigningContext.Credentials = cred;
                context.SigningContext.Request = request;
                singer!.Sign(context.SigningContext);
                request = context.SigningContext.Request;

                // save to result
                result.Expiration = context.SigningContext.Expiration;

                // signed headers
                // content-type, content-md5, x-oss- and additionalHeaders in sign v4
                var expect = new List<string> { "content-type", "content-md5" };
                if (singer is Signer.SignerV4)
                {
                    if (context.SigningContext.AdditionalHeaders != null)
                    {
                        expect.AddRange(context.SigningContext.AdditionalHeaders.Select(h => h.ToLowerInvariant()));
                    }
                    var now = DateTime.UtcNow;
                    if (result.Expiration - now > TimeSpan.FromDays(7))
                    {
                        throw new PresignExpirationException();
                    }
                }

                var signedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var header in request.Headers)
                {
                    var low = header.Key.ToLowerInvariant();
                    if (expect.Contains(low) || low.StartsWith("x-oss-")) signedHeaders[header.Key] = header.Value;
                }

                result.SignedHeaders = signedHeaders;
            }

            result.Url = request.RequestUri.AbsoluteUri;
            result.Method = request.Method;

            return result;
        }

        public void Dispose()
        {
            _executeStack.Dispose();
        }

        private static ClientOptions ResolveConfig(Configuration config)
        {
            var opt = new ClientOptions()
            {
                Product = Defaults.Product,
                Region = config.Region.SafeString(),
                Endpoint = ResolveEndpoint(config),
                Retryer = ResolveRetryer(config),
                Signer = ResolveSigner(config),
                CredentialsProvider = config.CredentialsProvider,
                HttpTransport = ResolveHttpTransport(config),
            };
            opt.AddressStyle = ResolveAddressStyle(config, opt.Endpoint);

            // if not set, use defaults
            opt.AdditionalHeaders = config.AdditionalHeaders ?? opt.AdditionalHeaders;
            opt.ReadWriteTimeout = config.ReadWriteTimeout ?? opt.ReadWriteTimeout;

            if (config.DisableUploadCrc64Check.GetValueOrDefault(false))
            {
                opt.FeatureFlags &= ~FeatureFlagsType.EnableCrc64CheckUpload;
            }

            if (config.DisableDownloadCrc64Check.GetValueOrDefault(false))
            {
                opt.FeatureFlags &= ~FeatureFlagsType.EnableCrc64CheckDownload;
            }

#if NETCOREAPP
            opt.RequestOnceTimeout = opt.ReadWriteTimeout;
#else
            // net4.7, 4.8 and netstandard use HttpClientHandler that does not support ConnectTimeout option.
            var connectTimeout = config.ConnectTimeout ?? Defaults.ConnectTimeout;
            opt.RequestOnceTimeout = (connectTimeout < opt.ReadWriteTimeout) ? opt.ReadWriteTimeout : connectTimeout;
#endif
            return opt;
        }

        private static Uri? ResolveEndpoint(Configuration config)
        {
            var disableSsl = config.DisableSsl.GetValueOrDefault(false);
            var endpoint = config.Endpoint.SafeString();
            var region = config.Region.SafeString();

            if (endpoint != "")
            {
                endpoint = endpoint.AddScheme(disableSsl);
            }
            else if (region.IsValidRegion())
            {
                string type;
                if (config.UseDualStackEndpoint.GetValueOrDefault(false))
                {
                    type = "dual-stack";
                }
                else if (config.UseInternalEndpoint.GetValueOrDefault(false))
                {
                    type = "internal";
                }
                else if (config.UseAccelerateEndpoint.GetValueOrDefault(false))
                {
                    type = "accelerate";
                }
                else
                {
                    type = "default";
                }

                endpoint = region.ToEndpoint(disableSsl, type);
            }

            return endpoint.ToUri();
        }

        private static Retry.IRetryer ResolveRetryer(Configuration config)
        {
            return config.Retryer ?? new Retry.StandardRetryer(maxAttempts: config.RetryMaxAttempts);
        }

        private static Signer.ISigner ResolveSigner(Configuration config)
        {
            return config.SignatureVersion.SafeString() switch
            {
                "v1" => new Signer.SignerV1(),
                _ => new Signer.SignerV4(),
            };
        }

        private static Transport.HttpTransport ResolveHttpTransport(Configuration config)
        {
            if (config.HttpTransport != null)
            {
                return config.HttpTransport;
            }

            var httpOpt = new Transport.HttpTransportOptions()
            {
                InsecureSkipVerify = config.InsecureSkipVerify,
                EnabledRedirect = config.EnabledRedirect,
                ConnectTimeout = config.ConnectTimeout,
            };

            // Proxy
            if (config.ProxyHost != null)
            {
                try
                {
                    var address = new Uri(config.ProxyHost);
                    httpOpt.HttpProxy = new WebProxy(address);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var client = Transport.HttpTransport.CreateCustomClient(httpOpt);
            return new Transport.HttpTransport(client);
        }

        private static AddressStyleType ResolveAddressStyle(Configuration config, Uri? endpoint)
        {
            var style = AddressStyleType.VirtualHosted;

            if (config.UseCName.GetValueOrDefault(false))
            {
                style = AddressStyleType.CName;
            }
            else if (config.UsePathStyle.GetValueOrDefault(false))
            {
                style = AddressStyleType.Path;
            }

            //if the endpoint is ip, set to path-style
            if (endpoint != null)
            {
                if (endpoint.IsHostIp())
                {
                    style = AddressStyleType.Path;
                }
            }

            return style;
        }

        private static string ResolveUserAgent(Configuration config)
        {
            // sys-info = {platform.system}/{platform.release}/{platform.machine}
            // user-agent =alibabacloud-dotnet-sdk-v2/sdk-version (sys-info)
            var osVersion = Environment.OSVersion.Version;
            var osPlatform = Environment.OSVersion.Platform;
            var runtimeVersion = Environment.Version;
            var info = $"{osPlatform}/{osVersion}/{runtimeVersion}";
            var sdkVersion = typeof(Configuration).Assembly.GetName().Version;
            var useragent = $"alibabacloud-dotnet-sdk-v2/{sdkVersion} ({info})";
            if (config.UserAgent != null)
            {
                return $"{useragent}/{config.UserAgent}";
            }
            return useragent;
        }

        private (RequestMessage first, ExecuteContext second) BuildRequestContext(
            OperationInput input,
            OperationOptions opOpts
        )
        {
            // default api options
            var context = new ExecuteContext
            {
                RetryMaxAttempts = opOpts.RetryMaxAttempts ?? Options.Retryer!.MaxAttempts(),
                RequestOnceTimeout = opOpts.ReadWriteTimeout ?? Options.RequestOnceTimeout,
                OnResponseMessage = new List<Action<ResponseMessage>> { OnServiceError }
            };

            // HttpCompletionOption
            if (input.OperationMetadata.TryGetValue("http-completion-option", out var value))
            {
                if (value is HttpCompletionOption s)
                {
                    context.HttpCompletionOption = s;
                }
            }

            // trackers
            List<Stream>? trackers = null;
            if (input.OperationMetadata.TryGetValue("opm-request-body-tracker", out value))
            {
                if (value is List<Stream> s)
                {
                    trackers = s;
                }
            }

            // response handlers
            if (input.OperationMetadata.TryGetValue("opm-response-handler", out value))
            {
                if (value is List<Action<ResponseMessage>> fns)
                {
                    foreach (var fn in fns)
                    {
                        context.OnResponseMessage.Add(fn);
                    }
                }
            }

            // signing context
            context.SigningContext = new()
            {
                Product = Options.Product,
                Region = Options.Region,
                Bucket = input.Bucket,
                Key = input.Key,
                AuthMethodQuery = (opOpts.AuthMethod ?? Options.AuthMethod) == AuthMethodType.Query,
                AdditionalHeaders = Options.AdditionalHeaders
            };

            // signing time from user
            if (input.OperationMetadata.TryGetValue("expiration-time", out var expirationTime))
            {
                if (expirationTime is DateTime time)
                {
                    context.SigningContext.Expiration = time;
                }
            }

            // request
            // request::host & path & query
            var endpoint = Options.Endpoint ?? throw new ArgumentException("Endpoint invalid.");
            var baseUrl = BuildHostPath(ref input, endpoint.Authority);
            var url = $"{endpoint.Scheme}://{baseUrl}";
            var query = CombineQueryString(input.Parameters);

            if (query != "")
            {
                url += "?" + query;
            }

            var uri = url.ToUri() ?? throw new Exception("BuildUrl fail");

            var request = new RequestMessage(input.Method, uri);

            // request::headers
            request.Headers.Add("User-Agent", InnerOptions.UserAgent);

            if (input.Headers != null)
            {
                foreach (var item in input.Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            // request::body
            if (trackers != null && input.Body != null)
            {
                request.Content = new TrackStream(input.Body, trackers.ToArray());
            }
            else
            {
                request.Content = input.Body;
            }

            return (request, context);
        }

        internal (int retry, TimeSpan timeout) GetRuntimeContext(OperationOptions? opOpts)
        {
            opOpts ??= _defaultOpOptions;
            var RetryMaxAttempts = opOpts.RetryMaxAttempts ?? Options.Retryer!.MaxAttempts();
            var RequestOnceTimeout = opOpts.ReadWriteTimeout ?? Options.RequestOnceTimeout;
            return (RetryMaxAttempts, RequestOnceTimeout);
        }

        private static void VerifyOperation(ref OperationInput input)
        {
            if (input.Bucket != null && !input.Bucket.IsValidBucketName())
            {
                throw new ArgumentException($"input.Bucket name is invalid, got {input.Bucket}.");
            }

            input.Key?.EnsureObjectNameValid($"{nameof(input)}.{nameof(input.Key)}");
        }

        private string BuildHostPath(ref OperationInput input, string baseUrl)
        {
            var paths = new List<string>();
            var host = baseUrl;

            if (input.Bucket != null)
            {
                switch (Options.AddressStyle)
                {
                    case AddressStyleType.Path:
                        paths.Add(input.Bucket);

                        if (input.Key == null)
                        {
                            paths.Add("");
                        }

                        break;
                    case AddressStyleType.CName:
                        break;
                    case AddressStyleType.VirtualHosted:
                    default:
                        host = $"{input.Bucket}.{host}";
                        break;
                }
            }

            if (input.Key != null)
            {
                paths.Add(input.Key.UrlEncodePath());
            }

            return $"{host}/{paths.JoinToString('/')}";
        }

        private static string CombineQueryString(IDictionary<string, string>? parameters)
        {
            if (parameters == null)
            {
                return "";
            }

            var isFirst = true;
            var queryString = new StringBuilder();

            foreach (var p in parameters)
            {
                if (!isFirst)
                    queryString.Append('&');

                isFirst = false;

                queryString.Append(p.Key.UrlEncode());

                if (!string.IsNullOrEmpty(p.Value))
                    queryString.Append('=').Append(p.Value.UrlEncode());
            }

            return queryString.ToString();
        }

        private static void OnServiceError(ResponseMessage response)
        {
            var statusCode = response.StatusCode;

            if (statusCode / 100 == 2) return;

            string? message = null;
            string? code = null;
            string? ec = null;
            string? requestId = null;
            var errorFields = new Dictionary<string, string>();
            var content = response.Content != null ? new StreamReader(response.Content).ReadToEnd() : "";

            if (string.IsNullOrEmpty(content) && response.Headers.TryGetValue("x-oss-err", out var val))
                content = Encoding.UTF8.GetString(Convert.FromBase64String(val));

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);
                var rootNode = xmlDoc.SelectSingleNode("Error");

                if (rootNode != null)
                {
                    foreach (XmlNode node in rootNode.ChildNodes) errorFields[node.Name] = node.InnerText;

                    errorFields.TryGetValue("Message", out message);
                    errorFields.TryGetValue("Code", out code);
                    errorFields.TryGetValue("RequestId", out requestId);
                    errorFields.TryGetValue("EC", out ec);
                }
                else
                {
                    message =
                        $"Not found tag <Error>, part response body {content.Substring(0, Math.Min(256, content.Length))}";
                }
            }
            catch (Exception)
            {
                //Ignore
            }

            code ??= "BadErrorResponse";

            message ??=
                $"Failed to parse xml from response body, part response body {content.Substring(0, Math.Min(256, content.Length))}";

            requestId ??= response.Headers.TryGetValue("x-oss-request-id", out var value) ? value : "";

            ec ??= response.Headers.TryGetValue("x-oss-ec", out value) ? value : "";

            var requestTime = response.Headers.TryGetValue("Date", out value) ? value : "";

            var details = new Dictionary<string, string>() {
                { "Code", code },
                { "Message", message },
                { "RequestId", requestId },
                { "Ec", ec },
                { "RequestTarget", $"{response.Request!.Method} {response.Request.RequestUri}" },
                { "TimeStamp", requestTime },
                { "Snapshot", content }
            };

            throw new ServiceException(statusCode, details, errorFields, response.Headers);
        }
    }

    internal static class OperationInputExtensions
    {

        public static void AddStreamTracker(this OperationInput input, Stream tracker)
        {
            List<Stream>? trackers = null;
            if (input.OperationMetadata.TryGetValue("opm-request-body-tracker", out var value))
            {
                if (value is List<Stream> val)
                {
                    trackers = val;
                }
            }
            trackers ??= new List<Stream>();
            trackers.Add(tracker);
            input.OperationMetadata["opm-request-body-tracker"] = trackers;
        }

        public static void AddResponseHandler(this OperationInput input, Action<ResponseMessage> handler)
        {
            List<Action<ResponseMessage>>? handlers = null;
            if (input.OperationMetadata.TryGetValue("opm-response-handler", out var value))
            {
                if (value is List<Action<ResponseMessage>> val)
                {
                    handlers = val;
                }
            }
            handlers ??= new List<Action<ResponseMessage>>();
            handlers.Add(handler);
            input.OperationMetadata["opm-response-handler"] = handlers;
        }
    }
}
