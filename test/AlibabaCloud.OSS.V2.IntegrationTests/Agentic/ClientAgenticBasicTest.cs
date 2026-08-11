using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// One agentic bucket shared by the Basic scenario. Teardown disables it and reaps
// backlog left disabled by earlier runs.
public class AgenticBucketFixture : IAsyncLifetime
{
    public AgenticBucketClient Client { get; private set; } = null!;
    public string Bucket { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (!Configured) return;
        Client = GetAgenticClient();
        Bucket = GenBucketName();
        var createResult = await Client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest
        {
            Bucket = Bucket,
            CreateAgenticBucketConfiguration = new CreateAgenticBucketConfiguration
            {
                StorageClass = "Standard",
                DataRedundancyType = "LRS"
            }
        });
        Assert.Equal(200, createResult.StatusCode);
    }

    public async Task DisposeAsync()
    {
        if (!Configured) return;
        await DisableAndReapAsync(Client, Bucket);
        Client.Dispose();
    }
}

// Basic scenario: Get / List(poll) / PutStatus(Enabled) over one shared bucket.
public class ClientAgenticBasicTest : IClassFixture<AgenticBucketFixture>
{
    private readonly AgenticBucketFixture _fx;
    public ClientAgenticBasicTest(AgenticBucketFixture fx) => _fx = fx;

    [Fact]
    public async Task TestGet()
    {
        if (!Configured) return;
        var getResult = await _fx.Client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = _fx.Bucket });
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.AgenticBucketInfo);
        Assert.Contains(_fx.Bucket, getResult.AgenticBucketInfo!.Name);
    }

    [Fact]
    public async Task TestList()
    {
        if (!Configured) return;
        // A newly created bucket may take a moment to appear in the list, so poll.
        var found = false;
        for (var attempt = 0; attempt < 5 && !found; attempt++)
        {
            if (attempt > 0) await Task.Delay(TimeSpan.FromSeconds(10));
            var paginator = _fx.Client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
            await foreach (var page in paginator.IterPageAsync())
            {
                if (page.AgenticBuckets == null) continue;
                foreach (var b in page.AgenticBuckets)
                {
                    if (b.Name != null && b.Name.Contains(_fx.Bucket)) found = true;
                }
            }
        }
        Assert.True(found, "created agentic bucket should appear in list");
    }

    [Fact]
    public async Task TestPutStatusEnabled()
    {
        if (!Configured) return;
        var putResult = await _fx.Client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
        {
            Bucket = _fx.Bucket,
            AgenticBucketStatus = new AgenticBucketStatus { Status = "Enabled" }
        });
        Assert.Equal(200, putResult.StatusCode);
    }

    [Fact]
    public async Task TestGetInvalidCredentials()
    {
        if (!Configured) return;
        using var client = GetInvalidAkClient();
        try
        {
            await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = _fx.Bucket });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.NotEqual(0, se!.StatusCode);
        }
    }

    [Fact]
    public async Task TestGetNotExist()
    {
        if (!Configured) return;
        try
        {
            await _fx.Client.GetAgenticBucketAsync(
                new GetAgenticBucketRequest { Bucket = "csharp-ab-not-exist-000000" });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.NotEqual(0, se!.StatusCode);
        }
    }

    [Fact]
    public async Task TestGetInvalidAccountId()
    {
        if (!Configured) return;
        using var client = GetBadAccountIdClient();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = _fx.Bucket }));
        Assert.Contains("account id", ex.Message);
    }
}
