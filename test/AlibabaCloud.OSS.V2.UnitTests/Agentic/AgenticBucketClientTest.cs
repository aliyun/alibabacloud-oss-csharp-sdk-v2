#nullable enable
using System.Net;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.UnitTests.Agentic;

public class AgenticBucketClientTest
{
    private static Configuration NewConfig(
        MockHttpMessageHandler mock,
        string? accountId = "123456",
        string? region = "cn-hangzhou"
    )
    {
        return new Configuration
        {
            Region = region,
            AccountId = accountId,
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mock)
        };
    }

    private static HttpResponseMessage OkXml(string body)
    {
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(body)
        };
    }

    [Fact]
    public async Task TestConstructorValidation()
    {
        // null config
        Assert.Throws<ArgumentNullException>(() => new AgenticBucketClient(null!));

        // empty account id and region are allowed at construction
        using var emptyClient = new AgenticBucketClient(
            new Configuration { CredentialsProvider = new AnonymousCredentialsProvider() });
        Assert.NotNull(emptyClient);

        // missing region is allowed at construction
        using var noRegion = new AgenticBucketClient(
            new Configuration { AccountId = "123456", CredentialsProvider = new AnonymousCredentialsProvider() });
        Assert.NotNull(noRegion);

        // non-digit account id: construction succeeds, error deferred to invoke
        var mock = new MockHttpMessageHandler();
        using var badClient = new AgenticBucketClient(NewConfig(mock, accountId: "abc123"));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            badClient.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = "my-agentic" }));
        Assert.Contains("account id", ex.Message);

        // valid
        using var client = new AgenticBucketClient(NewConfig(mock));
        Assert.NotNull(client);
    }

    [Fact]
    public async Task TestNewBucketSpaceClientValidation()
    {
        Assert.Throws<ArgumentNullException>(() => AgenticBucketClient.NewBucketSpaceClient(null!));

        // empty / missing config values are allowed at construction
        using var noRegion = AgenticBucketClient.NewBucketSpaceClient(
            new Configuration { AccountId = "123456", CredentialsProvider = new AnonymousCredentialsProvider() });
        Assert.NotNull(noRegion);

        // non-digit account id: construction succeeds, error deferred to invoke
        var mock = new MockHttpMessageHandler();
        using var badClient = AgenticBucketClient.NewBucketSpaceClient(NewConfig(mock, accountId: "abc123"));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            badClient.InvokeOperationAsync(new OperationInput
            {
                OperationName = "Test",
                Method = "GET",
                Bucket = "my-space"
            }));
        Assert.Contains("account id", ex.Message);

        using var client = AgenticBucketClient.NewBucketSpaceClient(NewConfig(mock));
        Assert.NotNull(client);
    }

    [Fact]
    public void TestBuildUserAgent()
    {
        Assert.Equal("agentic-client", AgenticBucketClient.BuildUserAgent(null));
        Assert.Equal("agentic-client", AgenticBucketClient.BuildUserAgent(""));
        Assert.Equal("agentic-client/custom", AgenticBucketClient.BuildUserAgent("custom"));
    }

    [Fact]
    public void TestAgenticProvider()
    {
        var provider = new AgenticProvider(
            new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "123456", "cn-hangzhou", "ab-apsr");

        Assert.Equal("my-agentic-123456-cn-hangzhou-ab-apsr",
            provider.BuildBucketName(new OperationInput { Bucket = "my-agentic" }));
        Assert.Null(provider.BuildBucketName(new OperationInput()));

        Assert.Equal(
            "https://my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com/",
            provider.BuildUrl(new OperationInput { Bucket = "my-agentic" }));
        Assert.Equal(
            "https://my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com/obj.txt",
            provider.BuildUrl(new OperationInput { Bucket = "my-agentic", Key = "obj.txt" }));
        Assert.Equal(
            "https://oss-cn-hangzhou.aliyuncs.com/",
            provider.BuildUrl(new OperationInput()));
    }

    [Fact]
    public void TestBucketSpaceHelper()
    {
        var helper = new BucketSpaceHelper(new Configuration { AccountId = "123456", Region = "cn-hangzhou" });
        Assert.Equal("my-space-123456-cn-hangzhou-bs-apsr", helper.ToBucketName("my-space"));

        Assert.Throws<ArgumentNullException>(() => new BucketSpaceHelper(null!));
    }

    [Fact]
    public async Task TestCreateAgenticBucketRequest()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest
        {
            Bucket = "my-agentic",
            CreateAgenticBucketConfiguration = new CreateAgenticBucketConfiguration
            {
                StorageClass = "Standard",
                DataRedundancyType = "LRS"
            }
        });

        Assert.NotNull(mock.LastRequest);
        Assert.Equal(HttpMethod.Put, mock.LastRequest.Method);
        Assert.Equal(
            "my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
        Assert.Contains("agenticBucket", mock.LastRequest.RequestUri.Query);
        Assert.Contains("agentic-client", mock.LastRequest.Headers.UserAgent.ToString());
    }

    [Fact]
    public async Task TestDeleteAgenticBucketRequest()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = "my-agentic" });

        Assert.Equal(HttpMethod.Delete, mock.LastRequest.Method);
        Assert.Equal(
            "my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
        Assert.Contains("agenticBucket", mock.LastRequest.RequestUri.Query);
    }

    [Fact]
    public async Task TestGetAgenticBucketResult()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        const string body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<AgenticBucketInfo>" +
            "<Name>my-agentic-123456-cn-hangzhou-ab-apsr</Name>" +
            "<Owner>1234</Owner>" +
            "<Region>cn-hangzhou</Region>" +
            "<StorageClass>Standard</StorageClass>" +
            "<DataRedundancyType>LRS</DataRedundancyType>" +
            "<Status>enabled</Status>" +
            "<ACL>private</ACL>" +
            "<Versioning>Enabled</Versioning>" +
            "<ServerSideEncryptionRule>" +
            "<ApplyServerSideEncryptionByDefault>" +
            "<SSEAlgorithm>KMS</SSEAlgorithm>" +
            "<KMSMasterKeyID>key-id</KMSMasterKeyID>" +
            "</ApplyServerSideEncryptionByDefault>" +
            "</ServerSideEncryptionRule>" +
            "</AgenticBucketInfo>";

        mock.Clear();
        mock.Responses = [OkXml(body)];

        var result = await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = "my-agentic" });

        Assert.Equal(HttpMethod.Get, mock.LastRequest.Method);
        Assert.NotNull(result.AgenticBucketInfo);
        Assert.Equal("my-agentic-123456-cn-hangzhou-ab-apsr", result.AgenticBucketInfo!.Name);
        Assert.Equal("cn-hangzhou", result.AgenticBucketInfo.Region);
        Assert.Equal("Standard", result.AgenticBucketInfo.StorageClass);
        Assert.Equal("enabled", result.AgenticBucketInfo.Status);
        Assert.Equal("private", result.AgenticBucketInfo.ACL);
        Assert.Equal("Enabled", result.AgenticBucketInfo.Versioning);
        Assert.NotNull(result.AgenticBucketInfo.ServerSideEncryptionRule?.ApplyServerSideEncryptionByDefault);
        Assert.Equal("KMS",
            result.AgenticBucketInfo.ServerSideEncryptionRule!.ApplyServerSideEncryptionByDefault!.SSEAlgorithm);
        Assert.Equal("key-id",
            result.AgenticBucketInfo.ServerSideEncryptionRule.ApplyServerSideEncryptionByDefault.KMSMasterKeyID);
    }

    [Fact]
    public async Task TestListAgenticBucketsResult()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        const string body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<ListAgenticBucketsResult>" +
            "<Region>cn-hangzhou</Region>" +
            "<Owner>1234</Owner>" +
            "<ContinuationToken></ContinuationToken>" +
            "<NextContinuationToken>next-token</NextContinuationToken>" +
            "<IsTruncated>true</IsTruncated>" +
            "<AgenticBuckets>" +
            "<AgenticBucket>" +
            "<Name>bucket-a</Name>" +
            "<StorageClass>Standard</StorageClass>" +
            "<DataRedundancyType>LRS</DataRedundancyType>" +
            "<CreateTime>2024-01-01T00:00:00.000Z</CreateTime>" +
            "</AgenticBucket>" +
            "<AgenticBucket>" +
            "<Name>bucket-b</Name>" +
            "<StorageClass>IA</StorageClass>" +
            "</AgenticBucket>" +
            "</AgenticBuckets>" +
            "</ListAgenticBucketsResult>";

        mock.Clear();
        mock.Responses = [OkXml(body)];

        var result = await client.ListAgenticBucketsAsync(new ListAgenticBucketsRequest { MaxKeys = 100 });

        Assert.Equal(HttpMethod.Get, mock.LastRequest.Method);
        // ListAgenticBuckets has no bucket, so host is the plain endpoint.
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", mock.LastRequest.RequestUri!.Host);
        Assert.Contains("agenticBucket", mock.LastRequest.RequestUri.Query);
        Assert.Contains("max-keys=100", mock.LastRequest.RequestUri.Query);

        Assert.Equal("cn-hangzhou", result.Region);
        Assert.Equal("1234", result.Owner);
        Assert.Equal("next-token", result.NextContinuationToken);
        Assert.True(result.IsTruncated);
        Assert.NotNull(result.AgenticBuckets);
        Assert.Equal(2, result.AgenticBuckets!.Count);
        Assert.Equal("bucket-a", result.AgenticBuckets[0].Name);
        Assert.Equal("Standard", result.AgenticBuckets[0].StorageClass);
        Assert.Equal("bucket-b", result.AgenticBuckets[1].Name);
        Assert.Equal("IA", result.AgenticBuckets[1].StorageClass);
    }

    [Fact]
    public async Task TestListBucketSpacesResult()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        const string body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<ListBucketSpacesResult>" +
            "<Owner><ID>1234</ID><DisplayName>owner-name</DisplayName></Owner>" +
            "<Prefix>abc</Prefix>" +
            "<MaxKeys>50</MaxKeys>" +
            "<ContinuationToken></ContinuationToken>" +
            "<NextContinuationToken>next</NextContinuationToken>" +
            "<StartAfter>space-0</StartAfter>" +
            "<IsTruncated>false</IsTruncated>" +
            "<BucketSpaces>" +
            "<BucketSpace>" +
            "<Name>space-1</Name>" +
            "<Location>oss-cn-hangzhou</Location>" +
            "<CreationDate>2024-01-01T00:00:00.000Z</CreationDate>" +
            "<StorageClass>Standard</StorageClass>" +
            "</BucketSpace>" +
            "</BucketSpaces>" +
            "</ListBucketSpacesResult>";

        mock.Clear();
        mock.Responses = [OkXml(body)];

        var result = await client.ListBucketSpacesAsync(new ListBucketSpacesRequest
        {
            Bucket = "my-agentic",
            Prefix = "abc",
            StartAfter = "space-0"
        });

        Assert.Equal(HttpMethod.Get, mock.LastRequest.Method);
        Assert.Equal(
            "my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
        Assert.Contains("bucketSpace", mock.LastRequest.RequestUri.Query);
        Assert.Contains("start-after=space-0", mock.LastRequest.RequestUri.Query);

        Assert.Equal("1234", result.Owner?.Id);
        Assert.Equal("owner-name", result.Owner?.DisplayName);
        Assert.Equal("abc", result.Prefix);
        Assert.Equal(50, result.MaxKeys);
        Assert.Equal("space-0", result.StartAfter);
        Assert.False(result.IsTruncated);
        Assert.NotNull(result.BucketSpaces);
        Assert.Single(result.BucketSpaces!);
        Assert.Equal("space-1", result.BucketSpaces![0].Name);
        Assert.Equal("oss-cn-hangzhou", result.BucketSpaces[0].Location);
        Assert.Equal("Standard", result.BucketSpaces[0].StorageClass);
    }

    [Fact]
    public async Task TestPutAgenticBucketStatusRequest()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
        {
            Bucket = "my-agentic",
            AgenticBucketStatus = new AgenticBucketStatus { Status = "enabled" }
        });

        Assert.Equal(HttpMethod.Put, mock.LastRequest.Method);
        Assert.Contains("status", mock.LastRequest.RequestUri!.Query);
    }

    [Fact]
    public async Task TestBucketSpaceClientResolvesName()
    {
        var mock = new MockHttpMessageHandler();
        using var client = AgenticBucketClient.NewBucketSpaceClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.PutObjectAsync(new AlibabaCloud.OSS.V2.Models.PutObjectRequest
        {
            Bucket = "my-space",
            Key = "obj.txt",
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("hello"))
        });

        Assert.Equal(
            "my-space-123456-cn-hangzhou-bs-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
        Assert.Contains("agentic-client", mock.LastRequest.Headers.UserAgent.ToString());
    }

    private static string LastBody(MockHttpMessageHandler mock)
    {
        return System.Text.Encoding.UTF8.GetString(mock.RequestBodies[mock.RequestBodies.Count - 1]);
    }

    // --- #2 request body XML serialization ---

    [Fact]
    public async Task TestCreateAgenticBucketRequestBody()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest
        {
            Bucket = "my-agentic",
            CreateAgenticBucketConfiguration = new CreateAgenticBucketConfiguration
            {
                StorageClass = "Standard",
                DataRedundancyType = "LRS"
            }
        });

        var body = LastBody(mock);
        Assert.Contains("<StorageClass>Standard</StorageClass>", body);
        Assert.Contains("<DataRedundancyType>LRS</DataRedundancyType>", body);
    }

    [Fact]
    public async Task TestPutAgenticBucketStatusRequestBody()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
        {
            Bucket = "my-agentic",
            AgenticBucketStatus = new AgenticBucketStatus { Status = "enabled" }
        });

        Assert.Contains("<Status>enabled</Status>", LastBody(mock));
    }

    // --- #3 argument validation (Ensure.NotNull -> ArgumentNullException) ---

    [Fact]
    public async Task TestMissingBucketThrows()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest()));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest()));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.GetAgenticBucketAsync(new GetAgenticBucketRequest()));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.ListBucketSpacesAsync(new ListBucketSpacesRequest()));
    }

    [Fact]
    public async Task TestMissingRequiredBodyThrows()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest { Bucket = "my-agentic" }));
    }

    // --- agentic name resolution must force virtual-hosted URLs regardless of AddressStyle ---

    [Fact]
    public async Task TestResolvedHostForcesVirtualHostedUnderPathStyle()
    {
        var mock = new MockHttpMessageHandler();
        var config = NewConfig(mock);
        config.UsePathStyle = true;
        using var client = new AgenticBucketClient(config);

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = "my-agentic" });

        // The full bucket name stays in the host, not the path.
        Assert.Equal(
            "my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
        Assert.DoesNotContain("my-agentic-123456-cn-hangzhou-ab-apsr", mock.LastRequest.RequestUri.AbsolutePath);
    }

    [Fact]
    public async Task TestResolvedHostForcesVirtualHostedUnderCName()
    {
        var mock = new MockHttpMessageHandler();
        var config = NewConfig(mock);
        config.UseCName = true;
        using var client = new AgenticBucketClient(config);

        mock.Clear();
        mock.Responses = [OkXml("")];

        await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = "my-agentic" });

        // CName would normally drop the bucket; agentic must still prepend the full name.
        Assert.Equal(
            "my-agentic-123456-cn-hangzhou-ab-apsr.oss-cn-hangzhou.aliyuncs.com",
            mock.LastRequest.RequestUri!.Host);
    }
}
