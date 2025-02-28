using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2 {
    public class ServiceException : Exception {
        private readonly IDictionary<string, string> _details;

        public ServiceException(
            int statusCode,
            IDictionary<string, string>? details,
            IDictionary<string, string>? errorFields = null,
            IDictionary<string, string>? headers = null
        ) : base(ToMessage(statusCode, details)) {
            StatusCode = statusCode;
            _details = details ?? new Dictionary<string, string>();
            ErrorFields = errorFields ?? new Dictionary<string, string>();
            Headers = headers ?? new Dictionary<string, string>();
        }

        public int StatusCode { get; }

        public string ErrorCode => _details.TryGetValue("Code", out var value) ? value : "";

        public string ErrorMessage => _details.TryGetValue("Message", out var value) ? value : "";

        public string Ec => _details.TryGetValue("Ec", out var value) ? value : "";

        public string RequestId => _details.TryGetValue("RequestId", out var value) ? value : "";

        public string TimeStamp => _details.TryGetValue("TimeStamp", out var value) ? value : "";

        public string RequestTarget => _details.TryGetValue("RequestTarget", out var value) ? value : "";

        public string Snapshot => _details.TryGetValue("Snapshot", out var value) ? value : "";

        public IDictionary<string, string> ErrorFields { get; }

        public IDictionary<string, string> Headers { get; }

        private static string ToMessage(
            int statusCode,
            IDictionary<string, string>? details
        ) {
            return
                "Error returned by Service.\n" +
                $"Http Status Code: {statusCode}\n" +
                $"Error Code: {GetValueDefault(details, "Code")}\n" +
                $"Request Id: {GetValueDefault(details, "RequestId")}\n" +
                $"Message: {GetValueDefault(details, "Message")}\n" +
                $"EC: {GetValueDefault(details, "Ec")}\n" +
                $"Timestamp: {GetValueDefault(details, "TimeStamp")}\n" +
                $"Request Endpoint: {GetValueDefault(details, "RequestTarget")}";
        }

        private static string GetValueDefault(IDictionary<string, string>? map, string name) {
            if (map == null) return "";
            return map.TryGetValue(name, out var value) ? value : "";
        }
    }

    public class InconsistentException : Exception {
        public InconsistentException(
                string ccrc,
                string scrc,
                string requestId,
                Exception? innerException = null) : base(
                $"crc is inconsistent, client {ccrc} server {scrc}, RequestId {requestId}",
                innerException) { }
    }

    public class NoRetryableInconsistentException : Exception {
        public NoRetryableInconsistentException(
                string ccrc,
                string scrc,
                string requestId,
                Exception? innerException = null) : base(
                $"crc is inconsistent, client {ccrc} server {scrc}, RequestId {requestId}",
                innerException) { }
    }

    public class RequestFailedException : Exception {
        public RequestFailedException(string message) : base(message) { }

        public RequestFailedException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RequestTimeoutException : Exception {
        public RequestTimeoutException(string message, Exception? innerException = null) : base(message, innerException) { }
    }

    public class OperationException : Exception {
        public OperationException(string name, Exception? innerException = null)
          : base($"operation error {name}: {innerException}", innerException) {
        }
    };

    public class PresignExpirationException : Exception {
        public PresignExpirationException() : base("Expires should be not greater than 604800 s (seven days)") { }
    }
}