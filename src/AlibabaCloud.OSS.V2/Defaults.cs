using System;

namespace AlibabaCloud.OSS.V2
{
    public readonly struct Defaults
    {
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

        // defaults for file-like prefetch (ReadOnlyFile)
        public const int AsyncReadBufferSize = 1 * 1024 * 1024;

        public const int SoftStartInitial = 512 * 1024;

        public const long DefaultPrefetchThreshold = 20L * 1024 * 1024;

        public const int DefaultPrefetchNum = 3;

        public const long DefaultPrefetchChunkSize = 6L * 1024 * 1024;

        public const long DefaultOutOfOrderReadThreshold = 3;

        // defaults for high-level transfer managers (Uploader/Downloader/Copier)

        /// <summary>Max part number, 10000.</summary>
        public const int MaxUploadParts = 10000;

        /// <summary>Max part size, 5GiB, for UploadPart.</summary>
        public const long MaxPartSize = 5L * 1024 * 1024 * 1024;

        /// <summary>Min part size, 100KiB, for UploadPart.</summary>
        public const long MinPartSize = 100L * 1024;

        /// <summary>Default part size, 6MiB.</summary>
        public const long DefaultPartSize = 6L * 1024 * 1024;

        /// <summary>Default part size for uploader uploads data.</summary>
        public const long DefaultUploadPartSize = DefaultPartSize;

        /// <summary>Default part size for downloader downloads object.</summary>
        public const long DefaultDownloadPartSize = DefaultPartSize;

        /// <summary>Default part size for copier copies object, 64MiB.</summary>
        public const long DefaultCopyPartSize = 64L * 1024 * 1024;

        /// <summary>Default parallel number.</summary>
        public const int DefaultParallel = 3;

        /// <summary>Default parallel for uploader uploads data.</summary>
        public const int DefaultUploadParallel = DefaultParallel;

        /// <summary>Default parallel for downloader downloads object.</summary>
        public const int DefaultDownloadParallel = DefaultParallel;

        /// <summary>Default parallel for copier copies object.</summary>
        public const int DefaultCopyParallel = DefaultParallel;

        /// <summary>Default threshold to use multipart copy in Copier, 200MiB.</summary>
        public const long DefaultCopyThreshold = 200L * 1024 * 1024;

        /// <summary>Temp file suffix.</summary>
        public const string TempFileSuffix = ".temp";

        /// <summary>Checkpoint file suffix for Downloader.</summary>
        public const string CheckpointFileSuffixDownloader = ".dcp";

        /// <summary>Checkpoint file suffix for Uploader.</summary>
        public const string CheckpointFileSuffixUploader = ".ucp";

        /// <summary>Checkpoint file magic.</summary>
        public const string CheckpointMagic = "92611BED-89E2-46B6-89E5-72F273D4B0A3";
    }
}
