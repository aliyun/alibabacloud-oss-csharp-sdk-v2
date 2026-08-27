using System.Text;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// One agentic bucket plus one shared bucket space for the Space scenario.
public class BucketSpaceFixture : IAsyncLifetime
{
    public AgenticBucketClient Client { get; private set; } = null!;
    public Client BsClient { get; private set; } = null!;
    public string Bucket { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (!Configured) return;
        Client = GetAgenticClient();
        BsClient = GetBucketSpaceClient();
        Bucket = GenBucketName();

        var createResult = await Client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = Bucket });
        Assert.Equal(200, createResult.StatusCode);

        // Bucket space creation requires the parent agentic bucket full name.
        var putBucketResult = await BsClient.PutBucketAsync(new Models.PutBucketRequest
        {
            Bucket = Bucket,
            AgenticBucket = BuildFullName(Bucket, "ab-apsr")
        });
        Assert.Equal(200, putBucketResult.StatusCode);
    }

    public async Task DisposeAsync()
    {
        if (!Configured) return;
        try { await BsClient.DeleteBucketAsync(new Models.DeleteBucketRequest { Bucket = Bucket }); }
        catch { /* best effort */ }
        await DisableAndReapAsync(Client, Bucket);
        BsClient.Dispose();
        Client.Dispose();
    }
}

// Space scenario: bucket space listing plus bucket/object interfaces through the
// bucket space client and the BucketSpaceHelper-driven plain client.
public class ClientAgenticSpaceTest : IClassFixture<BucketSpaceFixture>
{
    private readonly BucketSpaceFixture _fx;
    public ClientAgenticSpaceTest(BucketSpaceFixture fx) => _fx = fx;

    [Fact]
    public async Task TestListBucketSpaces()
    {
        if (!Configured) return;
        var listResult = await _fx.Client.ListBucketSpacesAsync(new ListBucketSpacesRequest { Bucket = _fx.Bucket });
        Assert.Equal(200, listResult.StatusCode);
    }

    [Fact]
    public async Task TestBucketLifecycle()
    {
        if (!Configured) return;
        var putAclResult = await _fx.BsClient.PutBucketAclAsync(new Models.PutBucketAclRequest
        {
            Bucket = _fx.Bucket,
            Acl = "private"
        });
        Assert.Equal(200, putAclResult.StatusCode);

        var getAclResult = await _fx.BsClient.GetBucketAclAsync(new Models.GetBucketAclRequest { Bucket = _fx.Bucket });
        Assert.Equal(200, getAclResult.StatusCode);
        Assert.Equal("private", getAclResult.AccessControlPolicy?.AccessControlList?.Grant);
    }

    [Fact]
    public async Task TestObjectLifecycle()
    {
        if (!Configured) return;
        var key = "csharp-sdk-test-object-" + GenBucketName();
        const string content = "hello world";
        try
        {
            var putObjectResult = await _fx.BsClient.PutObjectAsync(new Models.PutObjectRequest
            {
                Bucket = _fx.Bucket,
                Key = key,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            });
            Assert.Equal(200, putObjectResult.StatusCode);

            var getObjectResult = await _fx.BsClient.GetObjectAsync(new Models.GetObjectRequest
            {
                Bucket = _fx.Bucket,
                Key = key
            });
            Assert.Equal(200, getObjectResult.StatusCode);
            using var reader = new StreamReader(getObjectResult.Body!);
            Assert.Equal(content, await reader.ReadToEndAsync());
        }
        finally
        {
            try { await _fx.BsClient.DeleteObjectAsync(new Models.DeleteObjectRequest { Bucket = _fx.Bucket, Key = key }); }
            catch { /* best effort */ }
        }
    }

    // Drive the same space through a plain Client using a helper-built full name.
    [Fact]
    public async Task TestSpaceHelper()
    {
        if (!Configured) return;
        var helper = new BucketSpaceHelper(TestConfig());
        var fullName = helper.ToBucketName(_fx.Bucket);
        Assert.Equal(BuildFullName(_fx.Bucket, "bs-apsr"), fullName);

        using var plainClient = new Client(TestConfig());

        var getInfoResult = await plainClient.GetBucketInfoAsync(new Models.GetBucketInfoRequest { Bucket = fullName });
        Assert.Equal(200, getInfoResult.StatusCode);

        var key = "csharp-sdk-test-object-" + GenBucketName();
        const string content = "hello helper";
        try
        {
            var putObjectResult = await plainClient.PutObjectAsync(new Models.PutObjectRequest
            {
                Bucket = fullName,
                Key = key,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            });
            Assert.Equal(200, putObjectResult.StatusCode);

            var getObjectResult = await plainClient.GetObjectAsync(new Models.GetObjectRequest
            {
                Bucket = fullName,
                Key = key
            });
            Assert.Equal(200, getObjectResult.StatusCode);
            using var reader = new StreamReader(getObjectResult.Body!);
            Assert.Equal(content, await reader.ReadToEndAsync());
        }
        finally
        {
            try { await plainClient.DeleteObjectAsync(new Models.DeleteObjectRequest { Bucket = fullName, Key = key }); }
            catch { /* best effort */ }
        }
    }
}
