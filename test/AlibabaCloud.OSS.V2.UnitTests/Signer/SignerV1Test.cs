using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Signer;

public class SignerV1Test
{
    [Fact]
    public void TestAuthHeader()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://examplebucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("Content-MD5", "eB5eJF1ptWaXm4bijSPyxw==");
        request.Headers.Add("Content-Type", "text/html");
        request.Headers.Add("x-oss-meta-author", "alice");
        request.Headers.Add("x-oss-meta-magic", "abracadabra");
        request.Headers.Add("x-oss-date", "Wed, 28 Dec 2022 10:27:41 GMT");
        var signTime = DateTimeOffset.FromUnixTimeSeconds(1672223261).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "examplebucket",
            Key = "nelson",
            Request = request,
            Credentials = cred,
            SignTime = signTime,
        };

        var signer = new V2.Signer.SignerV1();

        signer.Sign(signCtx);

        var authPat = "OSS ak:kSHKmLxlyEAKtZPkJhG9bZb5k7M=";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);

        // With Signed Parameter
        uri = "http://examplebucket.oss-cn-hangzhou.aliyuncs.com?acl".ToUri();
        request = new RequestMessage("PUT", uri);
        request.Headers.Add("Content-MD5", "eB5eJF1ptWaXm4bijSPyxw==");
        request.Headers.Add("Content-Type", "text/html");
        request.Headers.Add("x-oss-meta-author", "alice");
        request.Headers.Add("x-oss-meta-magic", "abracadabra");
        request.Headers.Add("x-oss-date", "Wed, 28 Dec 2022 10:27:41 GMT");
        signTime = DateTimeOffset.FromUnixTimeSeconds(1672223261).UtcDateTime;

        signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "examplebucket",
            Key = "nelson",
            Request = request,
            Credentials = cred,
            SignTime = signTime,
        };

        signer.Sign(signCtx);

        authPat = "OSS ak:/afkugFbmWDQ967j1vr6zygBLQk=";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);

        // With signed & non-signed Parameter & non-signed headers
        uri = "http://examplebucket.oss-cn-hangzhou.aliyuncs.com?acl&non-resousce=123".ToUri();
        request = new RequestMessage("PUT", uri);
        request.Headers.Add("Content-MD5", "eB5eJF1ptWaXm4bijSPyxw==");
        request.Headers.Add("Content-Type", "text/html");
        request.Headers.Add("x-oss-meta-author", "alice");
        request.Headers.Add("x-oss-meta-magic", "abracadabra");
        request.Headers.Add("x-oss-date", "Wed, 28 Dec 2022 10:27:41 GMT");
        request.Headers.Add("User-Agent", "test");
        signTime = DateTimeOffset.FromUnixTimeSeconds(1672223261).UtcDateTime;

        signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "examplebucket",
            Key = "nelson",
            Request = request,
            Credentials = cred,
            SignTime = signTime,
        };

        signer.Sign(signCtx);

        authPat = "OSS ak:/afkugFbmWDQ967j1vr6zygBLQk=";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);

        // With sub-resource
        uri = "http://examplebucket.oss-cn-hangzhou.aliyuncs.com/?resourceGroup&non-resousce=null".ToUri();
        request = new RequestMessage("GET", uri);
        request.Headers.Add("x-oss-date", "Wed, 28 Dec 2022 10:27:41 GMT");
        signTime = DateTimeOffset.FromUnixTimeSeconds(1672223261).UtcDateTime;

        signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "examplebucket",
            Request = request,
            Credentials = cred,
            SignTime = signTime,
            SubResource = ["resourceGroup"],
        };

        signer.Sign(signCtx);

        authPat = "OSS ak:vkQmfuUDyi1uDi3bKt67oemssIs=";
        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);

    }

    [Fact]
    public void TestAuthHeaderToken()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk", "token");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://examplebucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("Content-MD5", "eB5eJF1ptWaXm4bijSPyxw==");
        request.Headers.Add("Content-Type", "text/html");
        request.Headers.Add("x-oss-meta-author", "alice");
        request.Headers.Add("x-oss-meta-magic", "abracadabra");
        request.Headers.Add("x-oss-date", "Wed, 28 Dec 2022 10:27:41 GMT");
        var signTime = DateTimeOffset.FromUnixTimeSeconds(1672223261).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "examplebucket",
            Key = "nelson",
            Request = request,
            Credentials = cred,
            SignTime = signTime,
        };

        var signer = new V2.Signer.SignerV1();

        signer.Sign(signCtx);

        var authPat = "OSS ak:H3PAlN3Vucn74tPVEqaQC4AnLwQ=";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);
        Assert.Equal("token", signCtx.Request.Headers["x-oss-security-token"]);
    }

    [Fact]
    public void TestAuthQuery()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com/key?versionId=versionId".ToUri();
        var request = new RequestMessage("GET", uri);

        var expiration = DateTimeOffset.FromUnixTimeSeconds(1699807420).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "key",
            Request = request,
            Credentials = cred,
            Expiration = expiration,
            AuthMethodQuery = true,
        };

        var signer = new V2.Signer.SignerV1();

        signer.Sign(signCtx);

        //Console.WriteLine(signCtx.Request.RequestUri.AbsoluteUri);

        var querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("ak", querys["OSSAccessKeyId"]);
        Assert.Equal("1699807420", querys["Expires"]);
        Assert.Equal("dcLTea+Yh9ApirQ8o8dOPqtvJXQ=", querys["Signature"]);
        Assert.Equal("versionId", querys["versionId"]);
    }

    [Fact]
    public void TestAuthQueryToken()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk", "token");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com/key%2B123?versionId=versionId".ToUri();
        var request = new RequestMessage("GET", uri);

        var expiration = DateTimeOffset.FromUnixTimeSeconds(1699808204).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "key+123",
            Request = request,
            Credentials = cred,
            Expiration = expiration,
            AuthMethodQuery = true,
        };

        var signer = new V2.Signer.SignerV1();

        signer.Sign(signCtx);

        //Console.WriteLine(signCtx.Request.RequestUri.AbsoluteUri);

        var querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("ak", querys["OSSAccessKeyId"]);
        Assert.Equal("1699808204", querys["Expires"]);
        Assert.Equal("jzKYRrM5y6Br0dRFPaTGOsbrDhY=", querys["Signature"]);
        Assert.Equal("versionId", querys["versionId"]);
        Assert.Equal("token", querys["security-token"]);
        Assert.Equal("key%2B123", signCtx.Request.RequestUri.GetPath());
    }
}
