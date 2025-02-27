using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2.Models {
    public abstract class RequestModel {
        internal object? InnerBody;
        internal string BodyFormat = "";

        /// <summary>
        /// case-insensitive dictionary
        /// </summary>
        public IDictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// case-sensitive dictionary
        /// </summary>
        public IDictionary<string, string> Parameters { get; internal set; } = new Dictionary<string, string>();
    }
}
