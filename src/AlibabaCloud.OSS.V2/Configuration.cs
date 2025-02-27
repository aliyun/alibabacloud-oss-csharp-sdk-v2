using AlibabaCloud.OSS.V2.Transport;
using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.V2 {
    public class Configuration
    {
        /// <summary>
        /// The region in which the bucket is located.
        /// </summary>
        public string? Region { get; set;}

        /// <summary>
        /// The domain names that other services can use to access OSS.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// RetryMaxAttempts specifies the maximum number attempts an API client will call
        /// an operation that fails with a retryable error.
        /// </summary>
        public int? RetryMaxAttempts { get; set; }

        /// <summary>
        /// Retryer guides how HTTP requests should be retried in case of recoverable failures.
        /// </summary>
        public Retry.IRetryer? Retryer { get; set; }

        /// <summary>
        /// The HTTP client to invoke API calls with. Defaults to client's default HTTP implementation if null.
        /// </summary>
        public HttpTransport? HttpTransport { get; set; }

        /// <summary>
        /// The credentials provider to use when signing requests.
        /// </summary>
        public Credentials.ICredentialsProvider? CredentialsProvider { get; set; }

        /// <summary>
        /// Allows you to enable the client to use path-style addressing, i.e., https://oss-cn-hangzhou.aliyuncs.com/bucket/key.
        /// By default, the oss client will use virtual hosted addressing i.e., https://bucket.oss-cn-hangzhou.aliyuncs.com/key.
        /// </summary>
        public bool? UsePathStyle { get; set; }

        /// <summary>
        /// If the endpoint is s CName, set this flag to true
        /// </summary>
        public bool? UseCName { get; set; }

        /// <summary>
        /// Connect timeout
        /// </summary>
        public TimeSpan? ConnectTimeout { get; set; }

        /// <summary>
        /// read and write timeout
        /// </summary>
        public TimeSpan? ReadWriteTimeout { get; set; }

        /// <summary>
        /// Skip server certificate verification
        /// </summary>
        public bool? InsecureSkipVerify { get; set; }

        /// <summary>
        /// Enable http redirect or not. Default is disable
        /// </summary>
        public bool? EnabledRedirect { get; set; }

        /// <summary>
        /// Flag of using proxy host.
        /// </summary>
        public string? ProxyHost { get; set; }

        /// <summary>
        /// Read the proxy setting from the environment variables.
        /// HTTP_PROXY, HTTPS_PROXY and NO_PROXY (or the lowercase versions thereof).
        /// HTTPS_PROXY takes precedence over HTTP_PROXY for https requests.
        /// </summary>
        ///public bool? ProxyFromEnvironment { get; set; }

        /// <summary>
        /// Authentication with OSS Signature Version, Defaults is "v4"
        /// </summary>
        public string? SignatureVersion { get; set; }

        /// <summary>
        /// DisableSSL forces the endpoint to be resolved as HTTP.
        /// </summary>
        public bool? DisableSsl { get; set; }

        /// <summary>
        /// Dual-stack endpoints are provided in some regions.
        /// This allows an IPv4 client and an IPv6 client to access a bucket by using the same endpoint.
        ///  Set this to `true` to use a dual-stack endpoint for the requests.
        /// </summary>
        public bool? UseDualStackEndpoint { get; set; }

        /// <summary>
        /// OSS provides the transfer acceleration feature to accelerate date transfers of data
        /// uploads and downloads across countries and regions.
        /// Set this to `true` to use a accelerate endpoint for the requests.
        /// </summary>
        public bool? UseAccelerateEndpoint { get; set; }

        /// <summary>
        /// You can use an internal endpoint to communicate between Alibaba Cloud services located  within the same
        /// region over the internal network. You are not charged for the traffic generated over the internal network.
        /// Set this to `true` to use a accelerate endpoint for the requests.
        /// </summary>
        public bool? UseInternalEndpoint { get; set; }

        /// <summary>
        /// Check data integrity of uploads via the crc64 by default.
        /// This feature takes effect for PutObject, AppendObject, UploadPart, Uploader.UploadFrom and Uploader.UploadFile
        /// Set this to `true` to disable this feature.
        /// </summary>
        public bool? DisableUploadCrc64Check { get; set; }

        /// <summary>
        /// Check data integrity of download via the crc64 by default.
        /// This feature only takes effect for Downloader.DownloadFile, GetObjectToFile
        /// Set this to `true` to disable this feature.
        /// </summary>
        public bool? DisableDownloadCrc64Check { get; set; }

        /// <summary>
        ///  Additional signable headers.
        /// </summary>
        public List<string>? AdditionalHeaders { get; set; }

        /// <summary>
        ///  The optional user specific identifier appended to the User-Agent header.
        /// </summary>
        public string? UserAgent { get; set; }


        public static Configuration LoadDefault()
        {
            return new Configuration();
        }
    }
}
