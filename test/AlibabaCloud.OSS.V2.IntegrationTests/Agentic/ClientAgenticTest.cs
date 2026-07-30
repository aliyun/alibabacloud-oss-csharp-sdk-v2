using System.Text;
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
}

// Lifecycle connectivity: on its own bucket (so disabling does not break other
// tests), PutStatus(Disabled) succeeds, then Delete is not yet ready (409).
public class ClientAgenticLifecycleTest
{
    [Fact]
    public async Task TestDisableThenDelete()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        var bucket = GenBucketName();

        var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
        Assert.Equal(200, createResult.StatusCode);

        try
        {
            var putResult = await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
            {
                Bucket = bucket,
                AgenticBucketStatus = new AgenticBucketStatus { Status = "Disabled" }
            });
            Assert.Equal(200, putResult.StatusCode);

            var threw = false;
            try
            {
                await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = bucket });
            }
            catch (Exception e)
            {
                threw = true;
                var se = e as ServiceException ?? e.InnerException as ServiceException;
                Assert.NotNull(se);
                Assert.True(se!.StatusCode == 409 || se.ErrorCode == "AgenticBucketNotReady",
                    $"expected AgenticBucketNotReady/409, got code={se.ErrorCode} status={se.StatusCode}");
            }
            Assert.True(threw, "immediate delete after disable should fail (not ready)");
        }
        finally
        {
            await ReapDisabledAsync(client);
        }
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

// Error propagation with invalid credentials. Create returns 403; Get/List are
// relaxed because the real service returns 404 for Get under invalid AK.
public class ClientAgenticServerErrorsTest
{
    [Fact]
    public async Task TestServerErrors()
    {
        if (!Configured) return;
        using var client = GetInvalidAkClient();
        var bucket = GenBucketName();

        // create with invalid AK
        try
        {
            await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se!.StatusCode);
            Assert.NotEmpty(se.RequestId);
        }

        // get with invalid AK
        try
        {
            await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = bucket });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.NotEqual(0, se!.StatusCode);
        }

        // list with invalid AK
        try
        {
            await client.ListAgenticBucketsAsync(new ListAgenticBucketsRequest());
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.NotEqual(0, se!.StatusCode);
        }
    }
}
