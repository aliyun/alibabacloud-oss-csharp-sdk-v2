using System.Net;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Retry;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Signer;
using AlibabaCloud.OSS.V2.Transport;
using System.Text;
using System.Globalization;

namespace AlibabaCloud.OSS.V2.UnitTests.Internal;


public class NonSeekableStream(Stream baseStream) : WrapperStream(baseStream) {
    public override bool CanSeek => false;
}


public class ClientImplTest {
    [Fact]
    public void TestDefaultConfiguration() {
        var config = Configuration.LoadDefault();
        config.Region              = "cn-hangzhou";
        config.CredentialsProvider = new AnonymousCredentialsProvider();
        var client = new ClientImpl(config);
        Assert.Equal(config, client.Config);

        //default
        Assert.Equal("oss", client.Options.Product);
        Assert.Equal("cn-hangzhou", client.Options.Region);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("https", client.Options.Endpoint.Scheme);

        Assert.Equal(AddressStyleType.VirtualHosted, client.Options.AddressStyle);
        Assert.Equal(Defaults.ReadWriteTimeout, client.Options.RequestOnceTimeout);
        Assert.Equal(Defaults.ReadWriteTimeout, client.Options.ReadWriteTimeout);
        Assert.Equal(AuthMethodType.Header, client.Options.AuthMethod);
        Assert.Equal(Defaults.FeatureFlags, client.Options.FeatureFlags);

        Assert.IsAssignableFrom<StandardRetryer>(client.Options.Retryer);
        Assert.Equal(Defaults.MaxAttpempts, client.Options.Retryer.MaxAttempts());

        Assert.IsAssignableFrom<SignerV4>(client.Options.Signer);
        Assert.IsAssignableFrom<AnonymousCredentialsProvider>(client.Options.CredentialsProvider);

        Assert.NotNull(client.Options.HttpTransport);
        Assert.Empty(client.Options.AdditionalHeaders);
    }

    [Fact]
    public void TestConfigSignatureVersion() {
        var config = Configuration.LoadDefault();
        config.Region              = "cn-hangzhou";
        config.CredentialsProvider = new AnonymousCredentialsProvider();

        //default
        var client = new ClientImpl(config);
        Assert.Null(config.SignatureVersion);
        Assert.IsAssignableFrom<SignerV4>(client.Options.Signer);

        // set to v1
        config.SignatureVersion = "v1";
        client                  = new(config);
        Assert.IsAssignableFrom<SignerV1>(client.Options.Signer);

        // set to v4
        config.SignatureVersion = "v4";
        client                  = new(config);
        Assert.IsAssignableFrom<SignerV4>(client.Options.Signer);

        // set to any string
        config.SignatureVersion = "any";
        client                  = new(config);
        Assert.IsAssignableFrom<SignerV4>(client.Options.Signer);
    }

    [Fact]
    public void TestConfigEndpoint() {
        var config = Configuration.LoadDefault();
        config.Region              = "cn-hangzhou";
        config.CredentialsProvider = new AnonymousCredentialsProvider();

        //default
        var client = new ClientImpl(config);
        Assert.Null(config.Endpoint);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("https", client.Options.Endpoint.Scheme);

        // internal
        config = new() {
            Region              = "cn-shanghai",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            UseInternalEndpoint = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-cn-shanghai-internal.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("https", client.Options.Endpoint.Scheme);

        // accelerate
        config = new() {
            Region                = "cn-hangzhou",
            CredentialsProvider   = new AnonymousCredentialsProvider(),
            UseAccelerateEndpoint = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-accelerate.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("https", client.Options.Endpoint.Scheme);

        // dual stack
        config = new() {
            Region               = "cn-shenzhen",
            CredentialsProvider  = new AnonymousCredentialsProvider(),
            UseDualStackEndpoint = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("cn-shenzhen.oss.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("https", client.Options.Endpoint.Scheme);

        // set endpoint
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            Endpoint            = "http://oss-cn-shenzhen.aliyuncs.com"
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-cn-shenzhen.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("http", client.Options.Endpoint.Scheme);

        // disable ssl
        config = new() {
            Region              = "cn-shanghai",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            UseInternalEndpoint = true,
            DisableSsl          = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal("oss-cn-shanghai-internal.aliyuncs.com", client.Options.Endpoint.Host);
        Assert.Equal("http", client.Options.Endpoint.Scheme);
    }

    [Fact]
    public void TestConfigAddressStyle() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AddressStyleType.VirtualHosted, client.Options.AddressStyle);

        // cname
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            UseCName            = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AddressStyleType.CName, client.Options.AddressStyle);

        // path-style
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            UsePathStyle        = true
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AddressStyleType.Path, client.Options.AddressStyle);

        // ip endpoint
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            Endpoint            = "127.0.0.1"
        };
        client = new(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AddressStyleType.Path, client.Options.AddressStyle);
    }

    [Fact]
    public void TestConfigAuthMethod() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AuthMethodType.Header, client.Options.AuthMethod);

        // auth query
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        client = new(config, void (o) => { o.AuthMethod = AuthMethodType.Query; });
        Assert.NotNull(client.Options.Endpoint);
        Assert.Equal(AuthMethodType.Query, client.Options.AuthMethod);
    }

    [Fact]
    public void TestConfigProduct() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.Equal("oss", client.Options.Product);

        // auth query
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        client = new(config, void (o) => { o.Product = "oss-cloudbox"; });
        Assert.Equal("oss-cloudbox", client.Options.Product);
    }

    [Fact]
    public void TestConfigRetryer() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.IsAssignableFrom<StandardRetryer>(client.Options.Retryer);
        Assert.Equal(Defaults.MaxAttpempts, client.Options.Retryer.MaxAttempts());

        // set retryer
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            Retryer             = new NopRetryer()
        };
        client = new(config);
        Assert.IsAssignableFrom<NopRetryer>(client.Options.Retryer);
        Assert.Equal(1, client.Options.Retryer.MaxAttempts());

        //set MaxAttempts in retryer
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            Retryer             = new StandardRetryer(5)
        };
        client = new(config);
        Assert.IsAssignableFrom<StandardRetryer>(client.Options.Retryer);
        Assert.Equal(5, client.Options.Retryer.MaxAttempts());

        //set MaxAttempts in configuration
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            RetryMaxAttempts    = 10
        };
        client = new(config);
        Assert.IsAssignableFrom<StandardRetryer>(client.Options.Retryer);
        Assert.Equal(10, client.Options.Retryer.MaxAttempts());
    }

    [Fact]
    public void TestTimeout() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.Equal(Defaults.ReadWriteTimeout, client.Options.ReadWriteTimeout);
        Assert.Equal(Defaults.ReadWriteTimeout, client.Options.RequestOnceTimeout);

        // set read-write timeout
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            ReadWriteTimeout = TimeSpan.FromSeconds(50)
        };
        client = new ClientImpl(config);
        Assert.Equal(TimeSpan.FromSeconds(50), client.Options.ReadWriteTimeout);
        Assert.Equal(TimeSpan.FromSeconds(50), client.Options.RequestOnceTimeout);

        // connect timeout > read-write timeout
#if NETCOREAPP
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            ReadWriteTimeout = TimeSpan.FromSeconds(50)
        };
        client = new ClientImpl(config);
        Assert.Equal(TimeSpan.FromSeconds(50), client.Options.ReadWriteTimeout);
        Assert.Equal(TimeSpan.FromSeconds(50), client.Options.RequestOnceTimeout);
#else
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            ConnectTimeout    = TimeSpan.FromSeconds(60),
            ReadWriteTimeout = TimeSpan.FromSeconds(50)
        };
        client = new ClientImpl(config);
        Assert.Equal(TimeSpan.FromSeconds(50), client.Options.ReadWriteTimeout);
        Assert.Equal(TimeSpan.FromSeconds(60), client.Options.RequestOnceTimeout);
#endif
    }

    [Fact]
    public void TestConfigHttpClient() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.NotNull(client.Options.HttpTransport);

        // Set InsecureSkipVerify, EnabledRedirect
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            InsecureSkipVerify = true,
            EnabledRedirect = true,
        };
        client = new ClientImpl(config);
        Assert.NotNull(client.Options.HttpTransport);
    }

    [Fact]
    public void TestProxyHost() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.NotNull(client.Options.HttpTransport);

        // set proxy
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            ProxyHost = "http://127.0.0.1:8080"
        };
        client = new ClientImpl(config);
        Assert.NotNull(client.Options.HttpTransport);

        // set invalid proxy
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            ProxyHost           = "123456"
        };
        client = new ClientImpl(config);
        Assert.NotNull(client.Options.HttpTransport);
    }


    [Fact]
    public void TestConfigUserAgent() {
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.StartsWith("alibabacloud-dotnet-sdk-v2/0.", client.InnerOptions.UserAgent);

        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            UserAgent           = "my-agent"
        };
        client = new(config);
        Assert.StartsWith("alibabacloud-dotnet-sdk-v2/0.", client.InnerOptions.UserAgent);
        Assert.EndsWith("/my-agent", client.InnerOptions.UserAgent);
    }

    [Fact]
    public void TestConfigCrcCheck() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        var flags  = Defaults.FeatureFlags;
        Assert.Equal(flags, client.Options.FeatureFlags);

        // disable download and upload crc check
        config = new() {
            Region                    = "cn-hangzhou",
            CredentialsProvider       = new AnonymousCredentialsProvider(),
            DisableDownloadCrc64Check = true,
            DisableUploadCrc64Check   = true
        };
        client = new(config);
        flags  = FeatureFlagsType.CorrectClockSkew | FeatureFlagsType.AutoDetectMimeType;
        Assert.Equal(flags, client.Options.FeatureFlags);

        // disable download
        config = new() {
            Region                    = "cn-hangzhou",
            CredentialsProvider       = new AnonymousCredentialsProvider(),
            DisableDownloadCrc64Check = true
        };
        client = new(config);

        flags = FeatureFlagsType.CorrectClockSkew |
            FeatureFlagsType.AutoDetectMimeType |
            FeatureFlagsType.EnableCrc64CheckUpload;
        Assert.Equal(flags, client.Options.FeatureFlags);

        // disable upload
        config = new() {
            Region                  = "cn-hangzhou",
            CredentialsProvider     = new AnonymousCredentialsProvider(),
            DisableUploadCrc64Check = true
        };
        client = new(config);

        flags = FeatureFlagsType.CorrectClockSkew |
            FeatureFlagsType.AutoDetectMimeType |
            FeatureFlagsType.EnableCrc64CheckDownload;
        Assert.Equal(flags, client.Options.FeatureFlags);
    }

    [Fact]
    public void TestConfigAdditionalHeaders() {
        //default
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider()
        };
        var client = new ClientImpl(config);
        Assert.Empty(client.Options.AdditionalHeaders);

        //set values
        config = new() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            AdditionalHeaders   = ["host", "content-length"]
        };
        client = new(config);
        Assert.NotEmpty(client.Options.AdditionalHeaders);
        Assert.Equal("host", client.Options.AdditionalHeaders[0]);
        Assert.Equal("content-length", client.Options.AdditionalHeaders[1]);
    }

    [Fact]
    public  async Task TestRetryMaxAttemptsClientOptions() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        // default max retry attempts is 3
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket"
        };

        try {
            await client.ExecuteAsync(input);
        }
        catch (Exception) {
            //Ignore
        }
        Assert.Equal(Defaults.MaxAttpempts, mockHandler.Requests.Count);
        Assert.Single(mockHandler.Responses);

        // max retry attempts is 3
        config.RetryMaxAttempts = 4;
        client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket"
        };

        try {
            await client.ExecuteAsync(input);
        }
        catch (Exception) {
            //Ignore
        }
        Assert.Equal(4, mockHandler.Requests.Count);
        Assert.Empty(mockHandler.Responses);
    }

    [Fact]
    public async Task TestRetryMaxAttemptsFromOperationOptions() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket"
        };

        try {
            // set max retry attempts from operation options
            var opOptions = new OperationOptions() {
                RetryMaxAttempts = 2
            };
            await client.ExecuteAsync(input, opOptions);
        }
        catch (Exception) {
            //Ignore
        }
        Assert.Equal(2, mockHandler.Requests.Count);
        Assert.Equal(2, mockHandler.Responses.Count);
    }

    [Fact]
    public async Task TestRetryMaxAttemptsNopRetryer() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
            Retryer = new NopRetryer()
        };
        var client = new ClientImpl(config);

        // set max retry attempts from operation options
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket"
        };

        try {
            var opOptions = new OperationOptions() {
                RetryMaxAttempts = 2
            };
            await client.ExecuteAsync(input, opOptions);
        }
        catch (Exception) {
            //Ignore
        }

        Assert.NotNull(mockHandler.Requests);
        Assert.Single(mockHandler.Requests);
        Assert.Equal(3, mockHandler.Responses.Count);
    }

    [Fact]
    public async Task TestNoRetryError() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket"
        };

        try {
            await client.ExecuteAsync(input);
        }
        catch (Exception) {
            //Ignore
        }

        Assert.NotNull(mockHandler.Requests);
        Assert.Single(mockHandler.Requests);
        Assert.Equal(3, mockHandler.Responses.Count);
    }

    [Fact]
    public async Task TestNoSeekableStream() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            },
            new() {
                StatusCode = HttpStatusCode.BadGateway,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket",
            Body = new NonSeekableStream(new MemoryStream(Encoding.UTF8.GetBytes("hello world")))
        };

        try {
            await client.ExecuteAsync(input);
        }
        catch (Exception) {
            //Ignore
        }

        Assert.NotNull(mockHandler.Requests);
        Assert.Single(mockHandler.Requests);
        Assert.Equal(3, mockHandler.Responses.Count);
    }

    [Fact]
    public async Task TestSignerV4() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket",
        };

        await client.ExecuteAsync(input);
        var headers = mockHandler.LastRequest.Headers;
        Assert.Single(mockHandler.Requests);
        Assert.StartsWith("OSS4-HMAC-SHA256 Credential=ak/", headers.Authorization.ToString());
    }

    [Fact]
    public async Task TestSignerV1() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
            SignatureVersion = "v1"
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket",
        };

        await client.ExecuteAsync(input);
        var headers = mockHandler.LastRequest.Headers;
        Assert.Single(mockHandler.Requests);
        Assert.StartsWith("OSS ak:", headers.Authorization.ToString());
    }

    [Fact]
    public async Task TestAnonymousRequest() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "PutBucketAcl",
            Method        = "PUT",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "application/xml" },
            },
            Parameters = new Dictionary<string, string> {
                { "acl", "" },
            },
            Bucket = "bucket",
        };

        await client.ExecuteAsync(input);
        var headers = mockHandler.LastRequest.Headers;
        Assert.Single(mockHandler.Requests);
        Assert.Null(headers.Authorization);
    }

    [Fact]
    public async Task TestAddressingModeVirtualHost() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        // no bucket & key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://my-bucket.oss-cn-hangzhou.aliyuncs.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket and key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
            Key = "my-key"
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://my-bucket.oss-cn-hangzhou.aliyuncs.com/my-key?key=value", mockHandler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task TestAddressingModePath() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
            UsePathStyle = true
        };
        var client = new ClientImpl(config);

        // no bucket & key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com/my-bucket/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket and key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
            Key = "my-key+123"
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com/my-bucket/my-key%2B123?key=value", mockHandler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task TestAddressingModeCName() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
            UseCName = true,
            Endpoint = "http://www.cname.com"
        };
        var client = new ClientImpl(config);

        // no bucket & key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://www.cname.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://www.cname.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket and key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
            Key = "my-key+123"
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://www.cname.com/my-key%2B123?key=value", mockHandler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task TestAddressingModeIpEndpoint() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
            Endpoint = "http://192.168.1.1:8080"
        };
        var client = new ClientImpl(config);

        // no bucket & key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://192.168.1.1:8080/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://192.168.1.1:8080/my-bucket/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // bucket and key
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
            Key = "my-key+123"
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://192.168.1.1:8080/my-bucket/my-key%2B123?key=value", mockHandler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task TestEndpointWithQuery() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
            Endpoint = "http://www.test.com/123?key=value"
        };

        // virtual host
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://my-bucket.www.test.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());

        // path
        config.UsePathStyle = true;
        client              = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
                { "key1", "value1" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://www.test.com/my-bucket/?key=value&key1=value1", mockHandler.LastRequest.RequestUri.ToString());

        //cname
        config.UsePathStyle = false;
        config.UseCName = true;
        client              = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }];

        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "my-bucket",
        };

        await client.ExecuteAsync(input);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        Assert.Equal("http://www.test.com/?key=value", mockHandler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task TestVerifyExecuteAsyncArgs() {
        var mockHandler = new MockHttpMessageHandler();

        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        // bucket is invalid
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }
        ];
        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.Contains("input.Bucket name is invalid", e.ToString());
        }
        Assert.Null(mockHandler.LastRequest);

        // key is invalid
        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
            Key = ""
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.Contains("input.Key is invalid", e.ToString());
        }
        Assert.Null(mockHandler.LastRequest);
    }

    [Fact]
    public async Task TestInvalidEndpoint() {
        var mockHandler = new MockHttpMessageHandler();

        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
            Endpoint = "??"
        };
        var client = new ClientImpl(config);

        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }
        ];
        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.Contains("Endpoint invalid.", e.ToString());
        }
        Assert.Null(mockHandler.LastRequest);
    }

    [Fact]
    public async Task TestServiceError() {
        var mockHandler = new MockHttpMessageHandler();

        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport       = new HttpTransport(mockHandler),
            RetryMaxAttempts = 1
        };
        var client = new ClientImpl(config);

        // error in xml
        var errXml = """
<?xml version="1.0" encoding="UTF-8"?>
<Error>
    <Code>InvalidAccessKeyId</Code>
    <Message>The OSS Access Key Id you provided does not exist in our records.</Message>
    <RequestId>id-1234</RequestId>
    <HostId>oss-cn-hangzhou.aliyuncs.com</HostId>
    <OSSAccessKeyId>ak</OSSAccessKeyId>
    <EC>0002-00000902</EC>
    <RecommendDoc>https://api.aliyun.com/troubleshoot?q=0002-00000902</RecommendDoc>
</Error>
""";
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent(errXml)
            }
        ];
        var input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("id-1234", se.RequestId);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Equal(7, se.ErrorFields.Count);
        }
        Assert.NotNull(mockHandler.LastRequest);

        // error in header
        var errXmlBase64 = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4NCjxFcnJvcj4NCiA8Q29kZT5JbnZhbGlkQWNjZXNzS2V5SWQ8L0NvZGU+DQogPE1lc3NhZ2U+VGhlIE9TUyBBY2Nlc3MgS2V5IElkIHlvdSBwcm92aWRlZCBkb2VzIG5vdCBleGlzdCBpbiBvdXIgcmVjb3Jkcy48L01lc3NhZ2U+DQogPEhvc3RJZD5vc3MtY24taGFuZ3pob3UuYWxpeXVuY3MuY29tPC9Ib3N0SWQ+DQogPE9TU0FjY2Vzc0tleUlkPmFrPC9PU1NBY2Nlc3NLZXlJZD4NCjwvRXJyb3I+";
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.Forbidden,
            Content    = new StringContent(""),
        };
        response.Headers.Add("x-oss-err", errXmlBase64);
        response.Headers.Add("x-oss-request-id", "req-id-123");
        response.Headers.Add("x-oss-ec", "0002-00000901");
        mockHandler.Clear();
        mockHandler.Responses = [response];
        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("req-id-123", se.RequestId);
            Assert.Equal("0002-00000901", se.Ec);
            Assert.Equal(4, se.ErrorFields.Count);
        }
        Assert.NotNull(mockHandler.LastRequest);

        // not Error xml format
        errXml = """
<?xml version="1.0" encoding="UTF-8"?>
<NotError>
 <Code>InvalidAccessKeyId</Code>
 <Message>The OSS Access Key Id you provided does not exist in our records.</Message>
 <RequestId>id-1234</RequestId>
</NotError>
""";
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent(errXml)
            }
        ];
        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("BadErrorResponse", se.ErrorCode);
            Assert.Contains("Not found tag <Error>, part response body", se.ErrorMessage);
        }
        Assert.NotNull(mockHandler.LastRequest);

        // invalid xml format
        errXml = "invalid xml";
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.Forbidden,
                Content    = new StringContent(errXml)
            }
        ];
        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("BadErrorResponse", se.ErrorCode);
            Assert.Contains("Failed to parse xml from response body", se.ErrorMessage);
        }
        Assert.NotNull(mockHandler.LastRequest);

        // null response content
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.Forbidden
            }
        ];
        input = new OperationInput {
            OperationName = "InvokeOperation",
            Method        = "PUT",
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
            Bucket = "bucket",
        };

        try {
            await client.ExecuteAsync(input);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("BadErrorResponse", se.ErrorCode);
            Assert.Contains("Failed to parse xml from response body", se.ErrorMessage);
        }
        Assert.NotNull(mockHandler.LastRequest);
    }

    [Fact]
    public void TestPresignInnerV4() {
        var mockHandler = new MockHttpMessageHandler();
        var opOptions   = new OperationOptions();

        // ak sk
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key        = "key",
        };
        var expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod                       = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        var result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);

        // default signed headers
        input = new OperationInput {
            OperationName = "PutObject",
            Method        = "PUT",
            Bucket        = "bucket",
            Key           = "key+123",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "text" },
                { "Content-MD5", "md5-123" },
                {"x-oss-meta-key1", "value1"},
                {"abc", "abc-value1"},
                {"abc-2", "abc-value2"},
            },
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };
        expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod                       = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("PUT", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key%2B123?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.Contains("key=value", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Equal(3, result.SignedHeaders.Count);
        Assert.Equal("text", result.SignedHeaders["Content-Type"]);
        Assert.Equal("md5-123", result.SignedHeaders["Content-MD5"]);
        Assert.Equal("value1", result.SignedHeaders["x-oss-meta-key1"]);

        // additional-headers
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
            AdditionalHeaders = ["Abc"]
        };
        client = new ClientImpl(config);
        input = new OperationInput {
            OperationName = "PutObject",
            Method        = "PUT",
            Bucket        = "bucket",
            Key           = "key+123",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "text" },
                { "Content-MD5", "md5-123" },
                {"x-oss-meta-key1", "value1"},
                {"abc", "abc-value1"},
                {"abc-2", "abc-value2"},
            },
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };
        expiration                                 = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod                       = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result                                     = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("PUT", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key%2B123?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.Contains("key=value", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Equal(4, result.SignedHeaders.Count);
        Assert.Equal("text", result.SignedHeaders["Content-Type"]);
        Assert.Equal("md5-123", result.SignedHeaders["Content-MD5"]);
        Assert.Equal("value1", result.SignedHeaders["x-oss-meta-key1"]);
        Assert.Equal("abc-value1", result.SignedHeaders["abc"]);

        // token
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk", "token"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        client = new ClientImpl(config);

        input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod                       = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.Contains("x-oss-security-token=token", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    [Fact]
    public void TestPresignInnerV4DefaultExpiration() {
        var mockHandler = new MockHttpMessageHandler();
        var opOptions   = new OperationOptions();

        // ak sk
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        opOptions.AuthMethod                       = AuthMethodType.Query;
        var result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.NotNull(result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=900", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    [Fact]
    public void TestPresignInnerV1() {
        var mockHandler = new MockHttpMessageHandler();
        var opOptions = new OperationOptions();

        // ak sk
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport = new HttpTransport(mockHandler),
            SignatureVersion = "v1"
        };
        var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method = "GET",
            Bucket = "bucket",
            Key = "key",
        };
        var expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        var result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("OSSAccessKeyId=ak", result.Url);
        Assert.Contains("Expires=", result.Url);
        Assert.Contains("Signature=", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);

        // default signed headers
        input = new OperationInput {
            OperationName = "PutObject",
            Method = "PUT",
            Bucket = "bucket",
            Key = "key+123",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "text" },
                { "Content-MD5", "md5-123" },
                {"x-oss-meta-key1", "value1"},
                {"abc", "abc-value1"},
                {"abc-2", "abc-value2"},
            },
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };
        expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("PUT", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key%2B123?", result.Url);
        Assert.Contains("OSSAccessKeyId=ak", result.Url);
        Assert.Contains("Expires=", result.Url);
        Assert.Contains("Signature=", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Equal(3, result.SignedHeaders.Count);
        Assert.Equal("text", result.SignedHeaders["Content-Type"]);
        Assert.Equal("md5-123", result.SignedHeaders["Content-MD5"]);
        Assert.Equal("value1", result.SignedHeaders["x-oss-meta-key1"]);

        // additional-headers
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
            SignatureVersion    = "v1",
            AdditionalHeaders = ["Abc"]
        };
        client = new ClientImpl(config);
        input = new OperationInput {
            OperationName = "PutObject",
            Method = "PUT",
            Bucket = "bucket",
            Key = "key+123",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Content-Type", "text" },
                { "Content-MD5", "md5-123" },
                {"x-oss-meta-key1", "value1"},
                {"abc", "abc-value1"},
                {"abc-2", "abc-value2"},
            },
            Parameters = new Dictionary<string, string> {
                { "key", "value" },
            },
        };
        expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("PUT", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key%2B123?", result.Url);
        Assert.Contains("OSSAccessKeyId=ak", result.Url);
        Assert.Contains("Expires=", result.Url);
        Assert.Contains("Signature=", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Equal(3, result.SignedHeaders.Count);
        Assert.Equal("text", result.SignedHeaders["Content-Type"]);
        Assert.Equal("md5-123", result.SignedHeaders["Content-MD5"]);
        Assert.Equal("value1", result.SignedHeaders["x-oss-meta-key1"]);

        // token
        config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk", "token"),
            HttpTransport = new HttpTransport(mockHandler),
            SignatureVersion = "v1"
        };
        client = new ClientImpl(config);

        input = new OperationInput {
            OperationName = "GetObject",
            Method = "GET",
            Bucket = "bucket",
            Key = "key",
        };
        expiration = DateTime.UtcNow.AddHours(1);
        opOptions.AuthMethod = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("OSSAccessKeyId=ak", result.Url);
        Assert.Contains("Expires=", result.Url);
        Assert.Contains("Signature=", result.Url);
        Assert.Contains("security-token=token", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    [Fact]
    public void TestPresignInnerV1DefaultExpiration() {
        var mockHandler = new MockHttpMessageHandler();
        var opOptions   = new OperationOptions();

    // ak sk
    var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
            SignatureVersion    = "v1"
        };
        var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        var expiration = DateTime.UtcNow.AddMinutes(15);
        opOptions.AuthMethod                       = AuthMethodType.Query;
        input.OperationMetadata["expiration-time"] = expiration;
        var result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.NotNull(result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("OSSAccessKeyId=ak", result.Url);
        Assert.Contains($"Expires={FormatUnixTime(expiration)}", result.Url);
        Assert.Contains("Signature=", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    [Fact]
    public void TestPresignMisc() {
        var mockHandler = new MockHttpMessageHandler();
        var opOptions   = new OperationOptions();

        // _defaultPresignOpOptions
        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        var expiration = DateTime.UtcNow.AddHours(1);
        input.OperationMetadata["expiration-time"] = expiration;
        var result = client.PresignInner(input);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);

        // Null CredentialsProvider
        config = new Configuration() {
            Region              = "cn-hangzhou",
            HttpTransport       = new HttpTransport(mockHandler),
        };
        client = new ClientImpl(config);

        input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        result = client.PresignInner(input, opOptions);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Null(result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key", result.Url);
        Assert.DoesNotContain("x-oss-date=", result.Url);
        Assert.DoesNotContain("x-oss-expires=", result.Url);
        Assert.DoesNotContain("x-oss-signature=", result.Url);
        Assert.DoesNotContain("x-oss-credential=ak%2F", result.Url);
        Assert.DoesNotContain("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.Null(result.SignedHeaders);

        // empty ak&sk
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("", ""),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        client = new ClientImpl(config);

        input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };

        try {
            client.PresignInner(input, opOptions);
            Assert.Fail("should not here");
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.ToString());
        }

        // invalid expiration-time type
        config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        client = new ClientImpl(config);

        input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        input.OperationMetadata["expiration-time"] = "invalid expiration-time type";
        result = client.PresignInner(input);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.NotNull(result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=900", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    [Fact]
    public void TestDispose() {
        var mockHandler = new MockHttpMessageHandler();

        var config = new Configuration() {
            Region              = "cn-hangzhou",
            CredentialsProvider = new StaticCredentialsProvide("ak", "sk"),
            HttpTransport       = new HttpTransport(mockHandler),
        };
        using var client = new ClientImpl(config);

        var input = new OperationInput {
            OperationName = "GetObject",
            Method        = "GET",
            Bucket        = "bucket",
            Key           = "key",
        };
        var expiration = DateTime.UtcNow.AddHours(1);
        input.OperationMetadata["expiration-time"] = expiration;
        var result = client.PresignInner(input);
        Assert.NotNull(result);
        Assert.Equal("GET", result.Method);
        Assert.Equal(expiration, result.Expiration);
        Assert.Contains("bucket.oss-cn-hangzhou.aliyuncs.com/key?", result.Url);
        Assert.Contains("x-oss-date=", result.Url);
        Assert.Contains("x-oss-expires=", result.Url);
        Assert.Contains("x-oss-signature=", result.Url);
        Assert.Contains("x-oss-credential=ak%2F", result.Url);
        Assert.Contains("x-oss-signature-version=OSS4-HMAC-SHA256", result.Url);
        Assert.NotNull(result.SignedHeaders);
        Assert.Empty(result.SignedHeaders);
    }

    private static string FormatUnixTime(DateTime time) {
        const long ticksOf1970 = 621355968000000000;
        return ((time.ToUniversalTime().Ticks - ticksOf1970) / 10000000L).ToString(CultureInfo.InvariantCulture);
    }

}