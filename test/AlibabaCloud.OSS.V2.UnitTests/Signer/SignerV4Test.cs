using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Signer;

public class SignerV4Test
{
    [Fact]
    public void TestAuthHeader()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "text/plain");
        request.Headers.Add("x-oss-content-sha256", "UNSIGNED-PAYLOAD");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702743657).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var authPat = "OSS4-HMAC-SHA256 Credential=ak/20231216/cn-hangzhou/oss/aliyun_v4_request,Signature=e21d18daa82167720f9b1047ae7e7f1ce7cb77a31e8203a7d5f4624fa0284afe";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);
    }

    [Fact]
    public void TestAuthHeaderToken()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk", "token");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "text/plain");
        request.Headers.Add("x-oss-content-sha256", "UNSIGNED-PAYLOAD");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702784856).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var authPat = "OSS4-HMAC-SHA256 Credential=ak/20231217/cn-hangzhou/oss/aliyun_v4_request,Signature=b94a3f999cf85bcdc00d332fbd3734ba03e48382c36fa4d5af5df817395bd9ea";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);
    }

    [Fact]
    public void TestAuthHeaderWithAdditionalHeaders()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "text/plain");
        request.Headers.Add("x-oss-content-sha256", "UNSIGNED-PAYLOAD");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702747512).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            AdditionalHeaders = new List<string> { "ZAbc", "abc" }
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var authPat = "OSS4-HMAC-SHA256 Credential=ak/20231216/cn-hangzhou/oss/aliyun_v4_request,AdditionalHeaders=abc;zabc,Signature=4a4183c187c07c8947db7620deb0a6b38d9fbdd34187b6dbaccb316fa251212f";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);

        // with default signed header
        request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "text/plain");
        request.Headers.Add("x-oss-content-sha256", "UNSIGNED-PAYLOAD");

        signTime = DateTimeOffset.FromUnixTimeSeconds(1702747512).UtcDateTime;

        signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            AdditionalHeaders = new List<string> { "x-oss-no-exist", "ZAbc", "x-oss-head1", "abc" }
        };

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        signer.Sign(signCtx);

        authPat = "OSS4-HMAC-SHA256 Credential=ak/20231216/cn-hangzhou/oss/aliyun_v4_request,AdditionalHeaders=abc;zabc,Signature=4a4183c187c07c8947db7620deb0a6b38d9fbdd34187b6dbaccb316fa251212f";

        Assert.Equal(authPat, signCtx.Request.Headers["Authorization"]);
    }

    [Fact]
    public void TestAuthQuery()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "application/octet-stream");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702781677).UtcDateTime;
        var expiration = DateTimeOffset.FromUnixTimeSeconds(1702782276).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            Expiration = expiration,
            AuthMethodQuery = true,
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("OSS4-HMAC-SHA256", querys["x-oss-signature-version"]);
        Assert.Equal("599", querys["x-oss-expires"]);
        Assert.Equal("ak/20231217/cn-hangzhou/oss/aliyun_v4_request", querys["x-oss-credential"]);
        Assert.Equal("a39966c61718be0d5b14e668088b3fa07601033f6518ac7b523100014269c0fe", querys["x-oss-signature"]);
        Assert.False(querys.ContainsKey("x-oss-additional-headers"));
    }

    [Fact]
    public void TestAuthQueryToken()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk", "token");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "application/octet-stream");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702785388).UtcDateTime;
        var expiration = DateTimeOffset.FromUnixTimeSeconds(1702785987).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            Expiration = expiration,
            AuthMethodQuery = true,
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("OSS4-HMAC-SHA256", querys["x-oss-signature-version"]);
        Assert.Equal("20231217T035628Z", querys["x-oss-date"]);
        Assert.Equal("599", querys["x-oss-expires"]);
        Assert.Equal("ak/20231217/cn-hangzhou/oss/aliyun_v4_request", querys["x-oss-credential"]);
        Assert.Equal("3817ac9d206cd6dfc90f1c09c00be45005602e55898f26f5ddb06d7892e1f8b5", querys["x-oss-signature"]);
        Assert.Equal("token", querys["x-oss-security-token"]);
        Assert.False(querys.ContainsKey("x-oss-additional-headers"));
    }

    [Fact]
    public void TestAuthQueryWithAdditionalHeaders()
    {
        var provider = new V2.Credentials.StaticCredentialsProvider("ak", "sk");
        var cred = provider.GetCredentials();

        //case 1
        var uri = "http://bucket.oss-cn-hangzhou.aliyuncs.com".ToUri();
        var request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "application/octet-stream");

        var signTime = DateTimeOffset.FromUnixTimeSeconds(1702783809).UtcDateTime;
        var expiration = DateTimeOffset.FromUnixTimeSeconds(1702784408).UtcDateTime;

        var signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            Expiration = expiration,
            AuthMethodQuery = true,
            AdditionalHeaders = new List<string> { "ZAbc", "abc" },
        };

        var parameters = new Dictionary<string, string> {
            { "param1", "value1" },
            { "+param1", "value3" },
            { "|param1", "value4" },
            { "+param2", "" },
            { "|param2", "" },
            { "param2", "" }
        };

        var queryStr = parameters
        .Select(
            x =>
            {
                return x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode();
            })
        .JoinToString('&');

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        var signer = new V2.Signer.SignerV4();

        signer.Sign(signCtx);

        var querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("OSS4-HMAC-SHA256", querys["x-oss-signature-version"]);
        Assert.Equal("20231217T033009Z", querys["x-oss-date"]);
        Assert.Equal("599", querys["x-oss-expires"]);
        Assert.Equal("ak/20231217/cn-hangzhou/oss/aliyun_v4_request", querys["x-oss-credential"]);
        Assert.Equal("6bd984bfe531afb6db1f7550983a741b103a8c58e5e14f83ea474c2322dfa2b7", querys["x-oss-signature"]);
        Assert.Equal("abc;zabc", querys["x-oss-additional-headers"]);
        Assert.False(querys.ContainsKey("x-oss-security-token"));

        // with default signed header
        request = new RequestMessage("PUT", uri);
        request.Headers.Add("x-oss-head1", "value");
        request.Headers.Add("abc", "value");
        request.Headers.Add("ZAbc", "value");
        request.Headers.Add("XYZ", "value");
        request.Headers.Add("content-type", "application/octet-stream");

        signTime = DateTimeOffset.FromUnixTimeSeconds(1702783809).UtcDateTime;
        expiration = DateTimeOffset.FromUnixTimeSeconds(1702784408).UtcDateTime;

        signCtx = new V2.Signer.SigningContext()
        {
            Bucket = "bucket",
            Key = "1234+-/123/1.txt",
            Request = request,
            Credentials = cred,
            Product = "oss",
            Region = "cn-hangzhou",
            SignTime = signTime,
            Expiration = expiration,
            AuthMethodQuery = true,
            AdditionalHeaders = new List<string> { "x-oss-no-exist", "abc", "x-oss-head1", "ZAbc" },
        };

        signCtx.Request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

        signer.Sign(signCtx);

        querys = signCtx.Request.RequestUri.GetQueryParameters();
        Assert.Equal("OSS4-HMAC-SHA256", querys["x-oss-signature-version"]);
        Assert.Equal("20231217T033009Z", querys["x-oss-date"]);
        Assert.Equal("599", querys["x-oss-expires"]);
        Assert.Equal("ak/20231217/cn-hangzhou/oss/aliyun_v4_request", querys["x-oss-credential"]);
        Assert.Equal("6bd984bfe531afb6db1f7550983a741b103a8c58e5e14f83ea474c2322dfa2b7", querys["x-oss-signature"]);
        Assert.Equal("abc;zabc", querys["x-oss-additional-headers"]);
        Assert.False(querys.ContainsKey("x-oss-security-token"));
    }
}
