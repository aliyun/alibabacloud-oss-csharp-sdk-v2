using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.v2.Models {
    public abstract class ResultModel {
        internal object? InnerBody;
        internal Type?   BodyType;
        internal string  BodyFormat = "";

        /// <summary>
        /// Gets The collection of http response header.
        /// It is a case-insensitive dictionary
        /// </summary>
        public IDictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the reason phrase sent by server.
        /// </summary>
        public string Status { get; internal set; } = "";

        /// <summary>
        /// Gets the status code of http response.
        /// </summary>
        public int StatusCode { get; internal set; } = 0;

        /// <summary>
        /// Gets the request id sent by oss server.
        /// </summary>
        public string RequestId => Headers.TryGetValue("x-oss-request-id", out var value) ? value : "";
    }
}
