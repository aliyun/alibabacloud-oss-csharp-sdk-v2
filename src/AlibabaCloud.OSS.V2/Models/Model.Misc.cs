using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2.Models
{
    /// <summary>
    /// The result for the presign operation.
    /// </summary>
    public sealed class PresignResult
    {
        /// <summary>
        /// The HTTP method, which corresponds to the operation.
        /// For example, the HTTP method of the GetObject operation is GET.
        /// </summary>
        public string? Method { get; internal set; }

        /// <summary>
        /// The pre-signed URL.
        /// </summary>
        public string? Url { get; internal set; }

        /// <summary>
        /// The time when the pre-signed URL expires.
        /// </summary>
        public DateTime? Expiration { get; internal set; }

        /// <summary>
        /// The request headers specified in the request.
        /// For example, if Content-Type is specified for PutObject, Content-Type is returned.
        /// </summary>
        public IDictionary<string, string>? SignedHeaders { get; internal set; }
    }
}
