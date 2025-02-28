namespace AlibabaCloud.OSS.V2.UnitTests;

public class ConfigurationTest {
    [Fact]
    public void TestWithRequiredArgs() {
        var region = "cn-hangzhou";
        var config = new Configuration() {
            Region = region
        };
        Assert.Equal(region, config.Region);
    }

    [Fact]
    public void TestWithOptionalArgs() {
        var config = new Configuration() {
            Region = "cn-hangzhou",
            Endpoint = "oss-cn-hangzhou.aliyuncs.com",
            SignatureVersion = "v4",
            CredentialsProvider = new V2.Credentials.AnonymousCredentialsProvider(),
            RetryMaxAttempts = 3,
            Retryer = new V2.Retry.NopRetryer(),
            HttpTransport = new V2.Transport.HttpTransport(),
            ConnectTimeout = TimeSpan.FromSeconds(10),
            ReadWriteTimeout = TimeSpan.FromSeconds(20),
            UseDualStackEndpoint = true,
            UseAccelerateEndpoint = false,
            UseInternalEndpoint = true,
            DisableSsl = false,
            InsecureSkipVerify = true,
            EnabledRedirect = true,
            UseCName = false,
            UsePathStyle = true,
            ProxyHost = "http://127.0.0.1:8080",
            DisableUploadCrc64Check = false,
            DisableDownloadCrc64Check = true,
            AdditionalHeaders = ["host"],
            UserAgent = "test"
        };

        Assert.Equal("cn-hangzhou", config.Region);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", config.Endpoint);
        Assert.Equal("v4", config.SignatureVersion);
        Assert.IsAssignableFrom<V2.Credentials.AnonymousCredentialsProvider>(config.CredentialsProvider);
        Assert.Equal(3, config.RetryMaxAttempts);
        Assert.IsAssignableFrom<V2.Retry.NopRetryer>(config.Retryer);
        Assert.NotNull(config.HttpTransport);
        Assert.Equal(TimeSpan.FromSeconds(10), config.ConnectTimeout);
        Assert.Equal(TimeSpan.FromSeconds(20), config.ReadWriteTimeout);
        Assert.Equal(true, config.UseDualStackEndpoint);
        Assert.Equal(false, config.UseAccelerateEndpoint);
        Assert.Equal(true, config.UseInternalEndpoint);
        Assert.Equal(false, config.DisableSsl);
        Assert.Equal(true, config.InsecureSkipVerify);
        Assert.Equal(true, config.EnabledRedirect);
        Assert.Equal(false, config.UseCName);
        Assert.Equal(true, config.UsePathStyle);
        Assert.Equal("http://127.0.0.1:8080", config.ProxyHost);
        Assert.Equal(false, config.DisableUploadCrc64Check);
        Assert.Equal(true, config.DisableDownloadCrc64Check);
        Assert.Single(config.AdditionalHeaders);
        Assert.Equal("host", config.AdditionalHeaders[0]);
        Assert.Equal("test", config.UserAgent);
    }

    [Fact]
    public void TestWithDefaultValues() {
        var config = Configuration.LoadDefault();
        Assert.Null(config.Region);
        Assert.Null(config.Endpoint);
        Assert.Null(config.SignatureVersion);
        Assert.Null(config.CredentialsProvider);
        Assert.Null(config.RetryMaxAttempts);
        Assert.Null(config.Retryer);
        Assert.Null(config.HttpTransport);
        Assert.Null(config.ConnectTimeout);
        Assert.Null(config.ReadWriteTimeout);
        Assert.Null(config.UseDualStackEndpoint);
        Assert.Null(config.UseAccelerateEndpoint);
        Assert.Null(config.UseInternalEndpoint);
        Assert.Null(config.DisableSsl);
        Assert.Null(config.InsecureSkipVerify);
        Assert.Null(config.EnabledRedirect);
        Assert.Null(config.UseCName);
        Assert.Null(config.UsePathStyle);
        Assert.Null(config.ProxyHost);
        Assert.Null(config.DisableUploadCrc64Check);
        Assert.Null(config.DisableDownloadCrc64Check);
        Assert.Null(config.AdditionalHeaders);
        Assert.Null(config.UserAgent);
    }
}