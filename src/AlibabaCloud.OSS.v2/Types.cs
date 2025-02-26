using System;
using System.Collections.Generic;
using System.IO;

namespace AlibabaCloud.OSS.v2 {
    public sealed class OperationInput {
        private IDictionary<string, object>? _metadata;

        public string OperationName { get; set; } = string.Empty;
        public string Method        { get; set; } = string.Empty;

        public IDictionary<string, string>? Headers    { get; set; }
        public IDictionary<string, string>? Parameters { get; set; }

        public Stream? Body { get; set; }

        public string? Bucket { get; set; }
        public string? Key    { get; set; }

        public IDictionary<string, object> OperationMetadata {
            get {
                _metadata ??= new Dictionary<string, object>();
                return _metadata;
            }
            set { _metadata = value; }
        }
    }

    public sealed class OperationOutput {
        public string                       Status            { get; set; } = string.Empty;
        public int                          StatusCode        { get; set; }
        public IDictionary<string, string>? Headers           { get; set; }
        public Stream?                      Body              { get; set; }
        public IDictionary<string, object>? OperationMetadata { get; set; }
        public OperationInput?              Input             { get; set; }
    }

    public sealed class OperationOptions {
        /// <summary>
        /// The maximum number attempts.
        /// </summary>
        public int?                  RetryMaxAttempts     { get; set; }

        /// <summary>
        /// read and write timeout
        /// </summary>
        public TimeSpan?             ReadWriteTimeout     { get; set; }

        /// <summary>
        /// The way in which it is signed
        /// </summary>
        public AuthMethodType? AuthMethod { get; set; }
    }

    public class RequestMessage {
        public Stream? Content { get; set; }

        public string Method { get; set; }

        public Uri RequestUri { get; set; }

        public IDictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public RequestMessage(string method, Uri requestUri) {
            Method = method;
            RequestUri = requestUri;
        }
    }

    public class ResponseMessage {
        public Stream? Content { get; set; }

        public string Status { get; set; }

        public int StatusCode { get; set; }

        public IDictionary<string, string> Headers { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public RequestMessage? Request { get; set; }

        public ResponseMessage(int statusCode, string status) {
            Status = status;
            StatusCode = statusCode;
        }
    }

    [Flags]
    public enum FeatureFlagsType {
        None = 0,

        /// <summary>
        /// If the client time is different from server time by more than about 15 minutes,
        /// the requests your application makes will be signed with the incorrect time, and the server will reject them.
        /// The feature to help to identify this case, and SDK will correct for clock skew.
        /// </summary>
        CorrectClockSkew = 0x1,

        EnableMd5 = 0x2,

        /// <summary>
        /// Content-Type is automatically added based on the object name if not specified.
        /// This feature takes effect for PutObject, AppendObject and InitiateMultipartUpload
        /// </summary>
        AutoDetectMimeType = 0x4,

        /// <summary>
        /// Check data integrity of uploads via the crc64.
        /// This feature takes effect for PutObject, AppendObject, UploadPart, Uploader.UploadFrom and Uploader.UploadFile
        /// </summary>
        EnableCrc64CheckUpload = 0x8,

        /// <summary>
        /// Check data integrity of downloads via the crc64.
        /// This feature takes effect for Downloader.DownloadFile
        /// </summary>
        EnableCrc64CheckDownload = 0x10,
    }

    public enum AddressStyleType {
        VirtualHosted = 0,

        Path = 1,

        CName = 2,
    }

    public enum AuthMethodType {
        Header = 0,

        Query = 1,
    }

    public delegate void ProgressFunc(long increment, long transferred, long total);

}