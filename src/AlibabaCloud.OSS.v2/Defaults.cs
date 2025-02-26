using System;

namespace AlibabaCloud.OSS.v2 {
    public readonly struct Defaults {
        // defaults for signing
        public const string Product = "oss";

        public const string SignatureVersion = "v4";

        // defaults for transport
        public static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

        public static readonly TimeSpan ReadWriteTimeout = TimeSpan.FromSeconds(20);

        public static readonly TimeSpan IdleConnectionTimeout = TimeSpan.FromSeconds(50);

        public static readonly TimeSpan ExpectContinueTimeout = TimeSpan.FromSeconds(1);

        public static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(30);

        public const int MaxConnections = 100;

        public const string HttpScheme = "https";

        // defaults for retryer
        public const int MaxAttpempts = 3;

        public static readonly TimeSpan MaxBackOff = TimeSpan.FromSeconds(20);

        public static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(200);

        // defaults for feature flags
        public const FeatureFlagsType FeatureFlags =
            FeatureFlagsType.CorrectClockSkew |
            FeatureFlagsType.AutoDetectMimeType |
            FeatureFlagsType.EnableCrc64CheckUpload |
            FeatureFlagsType.EnableCrc64CheckDownload;

        public const int DefaultCopyBufferSize = 81920;
    }
}