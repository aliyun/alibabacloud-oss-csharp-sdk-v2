using System;
using System.Net;

namespace AlibabaCloud.OSS.V2.Transport {
    public class HttpTransportOptions {
        public static readonly TimeSpan DEFAULT_CONNECT_TIMEOUT = TimeSpan.FromSeconds(20);

        public static readonly TimeSpan DEFAULT_IDLE_CONNECTION_TIMEOUT = TimeSpan.FromSeconds(50);

        public static readonly TimeSpan DEFAULT_EXPECT_CONTINUE_TIMEOUT = TimeSpan.FromSeconds(1);

        public static readonly TimeSpan DEFAULT_KEEP_ALIVE_TIMEOUT = TimeSpan.FromSeconds(30);

        public static readonly int DEFAULT_MAX_CONNECTIONS = 100;

        public TimeSpan? ConnectTimeout { get; set; }

        public TimeSpan? ExpectContinueTimeout { get; set; }

        public TimeSpan? IdleConnectionTimeout { get; set; }

        public TimeSpan? KeepAliveTimeout { get; set; }

        public int? MaxConnections { get; set; }

        public bool? EnabledRedirect { get; set; }

        public bool? InsecureSkipVerify { get; set; }

        public IWebProxy? HttpProxy { get; set; }

        public HttpTransportOptions Merge(HttpTransportOptions options) {
            if (options.ConnectTimeout != null) {
                ConnectTimeout = options.ConnectTimeout;
            }
            if (options.ExpectContinueTimeout != null) {
                ExpectContinueTimeout = options.ExpectContinueTimeout;
            }
            if (options.IdleConnectionTimeout != null) {
                IdleConnectionTimeout = options.IdleConnectionTimeout;
            }
            if (options.KeepAliveTimeout != null) {
                KeepAliveTimeout = options.KeepAliveTimeout;
            }
            if (options.MaxConnections != null) {
                MaxConnections = options.MaxConnections;
            }
            if (options.EnabledRedirect != null) {
                EnabledRedirect = options.EnabledRedirect;
            }
            if (options.InsecureSkipVerify != null) {
                InsecureSkipVerify = options.InsecureSkipVerify;
            }
            if (options.HttpProxy != null) {
                HttpProxy = options.HttpProxy;
            }
            return this;
        }
    }
}
