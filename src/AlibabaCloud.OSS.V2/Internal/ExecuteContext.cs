using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace AlibabaCloud.OSS.V2.Internal {
    internal class ExecuteContext {
        public int?                           RetryMaxAttempts         { get; set; }
        public TimeSpan                       RequestOnceTimeout       { get; set; }
        public HttpCompletionOption?          HttpCompletionOption     { get; set; }
        public CancellationToken              ApiCallCancellationToken { get; set; } = CancellationToken.None;
        public Signer.SigningContext?         SigningContext           { get; set; }
        public List<Action<ResponseMessage>>? OnResponseMessage        { get; set; }
    }
}