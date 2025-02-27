using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2.Signer {
    public class SigningContext
    {
        public string? Product { get; set;}

        public string? Region { get; set; }

        public string? Bucket { get; set; }

        public string? Key { get; set; }

        public RequestMessage? Request { get; set; }

        public Credentials.Credentials? Credentials { get; set; }

        public bool AuthMethodQuery { get; set; }

        public string? StringToSign { get; set; }

        public DateTime? Expiration { get; set; }

        public DateTime? SignTime { get; set; }

        public List<string>? AdditionalHeaders { get; set; }

        public List<string>? SubResource { get; set; }
    }
}
