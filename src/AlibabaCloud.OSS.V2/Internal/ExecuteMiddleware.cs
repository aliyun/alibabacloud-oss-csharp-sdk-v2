using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.Internal
{
    internal class TransportExecuteMiddleware : IExecuteMiddleware
    {
        private static readonly string[] ContentHeaders = {
            "Expires", "Content-Disposition", "Content-Encoding", "Content-Language",
            "Content-Length", "Content-Type", "Content-MD5"
        };

        private static readonly HashSet<string> ContentHeadersHash = new(
            ContentHeaders,
            StringComparer.InvariantCultureIgnoreCase
        );
        private readonly HttpTransport handler;

        public TransportExecuteMiddleware(Transport.HttpTransport handler)
        {
            this.handler = handler;
        }

        public async Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context)
        {
            using var cts = new CancellationTokenSource(context.RequestOnceTimeout);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, context.ApiCallCancellationToken);

            using var httpRequest = new HttpRequestMessage(ConvertMethod(request.Method), request.RequestUri);
            HttpResponseMessage? httpResponse = null;
            var completionOption = context.HttpCompletionOption ?? HttpCompletionOption.ResponseContentRead;
            var content = NopCloseRequestContent.StreamFor(request.Content, cts, context.RequestOnceTimeout);

            // Write data to the request stream.
            // Get & Head Method does not support HttpContent
            if (!(httpRequest.Method == HttpMethod.Get ||
                    httpRequest.Method == HttpMethod.Head))
                httpRequest.Content = content != null
                    ? new StreamContent(content)
                    : new ByteArrayContent(Array.Empty<byte>());

            // Write headers
            foreach (var item in request.Headers)
            {
                if (!ContentHeadersHash.Contains(item.Key))
                {
                    httpRequest.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    continue;
                }

                if (httpRequest.Content == null) continue;

                switch (item.Key.ToLower())
                {
                    case "content-disposition":
                        httpRequest.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(item.Value);
                        break;
                    case "content-encoding":
                        httpRequest.Content.Headers.ContentEncoding.Add(item.Value);
                        break;
                    case "content-language":
                        httpRequest.Content.Headers.ContentLanguage.Add(item.Value);
                        break;
                    case "content-type":
                        httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(item.Value);
                        break;
                    case "content-md5":
                        httpRequest.Content.Headers.ContentMD5 = Convert.FromBase64String(item.Value);
                        break;
                    case "content-length":
                        httpRequest.Content.Headers.ContentLength = Convert.ToInt64(item.Value);
                        break;
                    case "expires":
                        if (DateTime.TryParse(
                                item.Value,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var expires
                            ))
                            httpRequest.Content.Headers.Expires = expires;
                        break;
                }
            }

            try
            {
                httpResponse = await handler.SendAsync(httpRequest, completionOption, linkedCts.Token).ConfigureAwait(false);

                Stream? contentStream = null;
                if (httpResponse.Content != null)
                {

                    contentStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    // allways save reponse body into memroy when status code is not 2xx(not include 203)
                    var statusCode = (int)httpResponse.StatusCode;
                    if ((statusCode == 203 || statusCode / 100 != 2) && !contentStream.CanSeek)
                    {
                        try
                        {
                            if (contentStream.CanTimeout)
                            {
                                contentStream.ReadTimeout = (int)context.RequestOnceTimeout.TotalMilliseconds;
                            }
                            var stream = new MemoryStream();
#if NET5_0_OR_GREATER
                            await contentStream.CopyToAsync(stream, linkedCts.Token).ConfigureAwait(false);
#else
                            await contentStream.CopyToAsync(stream).ConfigureAwait(false);
#endif
                            contentStream = stream;
                            contentStream.Seek(0, SeekOrigin.Begin);
                        }
                        finally
                        {
                            httpResponse.Content.Dispose();
                        }
                    }
                }

                var response = new ResponseMessage((int)httpResponse.StatusCode, httpResponse.ReasonPhrase!)
                {
                    Request = request,
                    Content = contentStream
                };

                // headers
                foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponse.Headers) response.Headers[header.Key] = string.Join(",", header.Value);

                if (httpResponse.Content != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponse.Content.Headers) response.Headers[header.Key] = string.Join(",", header.Value);
                }

                return response;
            }
            catch (OperationCanceledException e) when (cts.Token.IsCancellationRequested)
            {
                throw CancellationHelper.CreateRequestTimeoutException(e, context.RequestOnceTimeout);
            }
            catch (HttpRequestException e)
            {
                throw new RequestFailedException(e.Message, e);
            }
        }

        private static HttpMethod ConvertMethod(string method)
        {
            return method.ToLower() switch
            {
                "delete" => HttpMethod.Delete,
                "get" => HttpMethod.Get,
                "head" => HttpMethod.Head,
                "options" => HttpMethod.Options,
                "post" => HttpMethod.Post,
                "put" => HttpMethod.Put,
                _ => throw new InvalidCastException()
            };
        }
    }

    internal class ResponseCheckerExecuteMiddleware : IExecuteMiddleware
    {
        private readonly IExecuteMiddleware nextHandler;

        public ResponseCheckerExecuteMiddleware(IExecuteMiddleware nextHandler)
        {
            this.nextHandler = nextHandler;
        }

        public async Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context)
        {
            var response = await nextHandler.ExecuteAsync(request, context).ConfigureAwait(false);
            OnResponseMessage(response, context);
            return response;
        }

        private static void OnResponseMessage(ResponseMessage message, ExecuteContext context)
        {
            if (context.OnResponseMessage == null)
            {
                return;
            }
            foreach (var fn in context.OnResponseMessage) fn(message);
        }
    }

    internal class SignerExecuteMiddleware : IExecuteMiddleware
    {
        private readonly Signer.ISigner _signer;
        private readonly IExecuteMiddleware nextHandler;
        private readonly ICredentialsProvider? provider;

        public SignerExecuteMiddleware(
            IExecuteMiddleware nextHandler,
            Signer.ISigner? signer,
            Credentials.ICredentialsProvider? provider
        )
        {
            this.nextHandler = nextHandler;
            this.provider = provider;
            _signer = signer ?? new Signer.NopSigner();
        }

        public Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context)
        {
            if (provider != null &&
                provider is not AnonymousCredentialsProvider &&
                context.SigningContext != null)
            {
                var cred = provider.GetCredentials();
                if (!cred.HasKeys) throw new("Credentials is null or empty");
                context.SigningContext.Credentials = cred;
                context.SigningContext.Request = request;
                _signer.Sign(context.SigningContext);
                request = context.SigningContext.Request;
            }

            return nextHandler.ExecuteAsync(request, context);
        }
    }

    internal class RetryerExecuteMiddleware : IExecuteMiddleware
    {
        private readonly IExecuteMiddleware _nextHandler;
        private readonly Retry.IRetryer _retryer;
        private readonly int? _attempts;

        public RetryerExecuteMiddleware(IExecuteMiddleware nextHandler, Retry.IRetryer? retryer)
        {
            _nextHandler = nextHandler;
            _retryer = retryer ?? new Retry.NopRetryer();
            if (_retryer is Retry.NopRetryer) _attempts = 1;
        }

        public async Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context)
        {
            Exception? lastError;
            var body = request.Content ?? new MemoryStream();
            var attempts = _attempts ?? context.RetryMaxAttempts;
            var bodyPos = body.CanSeek ? body.Position : 0;

            for (var i = 0; ; i++)
            {
                try
                {
                    return await _nextHandler.ExecuteAsync(request, context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }

                if (i + 1 >= attempts) break;

                if (context.ApiCallCancellationToken.IsCancellationRequested) break;

                if (!body.CanSeek) break;

                if (!_retryer.IsErrorRetryable(lastError)) break;

                // delay
                var delay = _retryer.RetryDelay(i + 1, lastError);
                await Task.Delay(delay, context.ApiCallCancellationToken).ConfigureAwait(false);
                if (context.ApiCallCancellationToken.IsCancellationRequested) break;

                //Do Reset
                body.Seek(bodyPos, SeekOrigin.Begin);
                //reset signing time
            }

            throw lastError;
        }
    }
}
