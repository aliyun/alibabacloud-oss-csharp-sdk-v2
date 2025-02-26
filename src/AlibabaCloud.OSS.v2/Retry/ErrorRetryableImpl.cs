using System;

namespace AlibabaCloud.OSS.v2.Retry {
    internal class HttpStatusCodeRetryable : IErrorRetryable {
        private readonly int[] _statusCodes = new int[] { 401, 408, 429 };

        public bool IsErrorRetryable(Exception error) {
            if (error is ServiceException exception) {
                var statusCode = exception.StatusCode;
                if (statusCode >= 500) return true;

                foreach (var code in _statusCodes)
                    if (statusCode == code)
                        return true;
            }

            return false;
        }
    }

    internal class ServiceErrorCodeRetryable : IErrorRetryable {
        private readonly string[] _errorCodes = { "RequestTimeTooSkewed", "BadRequest" };

        public bool IsErrorRetryable(Exception error) {
            if (error is ServiceException exception)
                foreach (var code in _errorCodes)
                    if (exception.ErrorCode == code)
                        return true;

            return false;
        }
    }

    internal class ClientExceptionRetryable : IErrorRetryable {
        public bool IsErrorRetryable(Exception error) {
            return error switch {
                InconsistentException  => true,
                RequestFailedException => true,
                RequestTimeoutException => true,
                _ => false
            };
        }
    }
}