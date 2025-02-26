
using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.v2 {
    public class ClientOptions
    {
        public string Product { get; set; } = "";

        public string Region { get; set; } = "";

        public Uri? Endpoint { get; set; }

        public Retry.IRetryer? Retryer { get; set; }

        public Signer.ISigner? Signer { get; set; }

        public Credentials.ICredentialsProvider? CredentialsProvider { get; set; }

        public Transport.HttpTransport? HttpTransport { get; set; }

        public AddressStyleType AddressStyle { get; set; }

        public AuthMethodType AuthMethod { get; set; }

        public TimeSpan ReadWriteTimeout { get; set; } = Defaults.ReadWriteTimeout;

        public FeatureFlagsType FeatureFlags { get; set; } = Defaults.FeatureFlags;

        public List<string> AdditionalHeaders { get; set; } = new List<string>();

        internal TimeSpan RequestOnceTimeout { get; set; } = Defaults.ReadWriteTimeout;
    }
}
