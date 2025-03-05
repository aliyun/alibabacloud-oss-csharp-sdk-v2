namespace AlibabaCloud.OSS.V2.UnitTests.Credentials;

public class CredentialsTest
{

    [Fact]
    public void TestCredentials()
    {
        V2.Credentials.Credentials cred;

        // empty 
        cred = new V2.Credentials.Credentials("", "");
        Assert.Equal("", cred.AccessKeyId);
        Assert.Equal("", cred.AccessKeySecret);
        Assert.Equal("", cred.SecurityToken);
        Assert.False(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);

        // long-term Credentials
        cred = new V2.Credentials.Credentials("ak", "sk");
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);

        // short-term Credentials
        cred = new V2.Credentials.Credentials("ak", "sk", "token");
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("token", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);

        // short-term Credentials with expiration time
        var expiration = DateTime.UtcNow.AddMinutes(10);
        cred = new V2.Credentials.Credentials("ak", "sk", "token", expiration);
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("token", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.NotNull(cred.Expiration);
        Assert.False(cred.IsExpired);

        expiration = DateTime.UtcNow.AddMinutes(-10);
        cred = new V2.Credentials.Credentials("ak", "sk", "token", expiration);
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("token", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.NotNull(cred.Expiration);
        Assert.True(cred.IsExpired);
    }

    [Fact]
    public void TestStaticCredentialsProvide()
    {
        var provider = new V2.Credentials.StaticCredentialsProvide("ak", "sk");
        var cred = provider.GetCredentials();
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);

        provider = new V2.Credentials.StaticCredentialsProvide("ak", "sk", "token");
        cred = provider.GetCredentials();
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("token", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);
    }

    [Fact]
    public void TestAnonymousCredentialsProvider()
    {
        var provider = new V2.Credentials.AnonymousCredentialsProvider();
        var cred = provider.GetCredentials();
        Assert.Equal("", cred.AccessKeyId);
        Assert.Equal("", cred.AccessKeySecret);
        Assert.Equal("", cred.SecurityToken);
        Assert.False(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);
    }

    [Fact]
    public void TestCredentialsProvideFunc()
    {

        var provider = new V2.Credentials.CredentialsProvideFunc(() => new V2.Credentials.Credentials("ak", "sk"));
        var cred = provider.GetCredentials();
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);

        provider = new V2.Credentials.CredentialsProvideFunc(() => new V2.Credentials.Credentials("ak", "sk", "token"));
        cred = provider.GetCredentials();
        Assert.Equal("ak", cred.AccessKeyId);
        Assert.Equal("sk", cred.AccessKeySecret);
        Assert.Equal("token", cred.SecurityToken);
        Assert.True(cred.HasKeys);
        Assert.Null(cred.Expiration);
        Assert.False(cred.IsExpired);
    }
}
