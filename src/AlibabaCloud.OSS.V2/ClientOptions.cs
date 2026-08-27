
using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2
{
    public class ClientOptions
    {
        public string Product { get; set; } = "";

        public string Region { get; set; } = "";

        public string? AccountId { get; set; }

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

        /// <summary>
        /// Resolves the bucket name used for signing. When set, the returned value replaces the
        /// bucket name in the signing context, while the operation input keeps its original name.
        /// </summary>
        public IBucketNameResolver? BucketNameResolver { get; set; }

        /// <summary>
        /// Builds the request URL for an operation. When set, it overrides the default
        /// address-style based URL construction.
        /// </summary>
        public IEndpointProvider? EndpointProvider { get; set; }

        internal TimeSpan RequestOnceTimeout { get; set; } = Defaults.ReadWriteTimeout;
    }
}
