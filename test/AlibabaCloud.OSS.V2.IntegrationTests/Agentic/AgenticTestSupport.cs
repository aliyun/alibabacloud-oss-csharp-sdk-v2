using System.Text;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Credentials;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// Shared helpers for the agentic integration tests: client factories, name
// builders, and the prefix-based reaper that bounds the backlog left by the
// two-phase (Disable -> wait ~24h -> Delete) bucket lifecycle.
internal static class AgenticTestSupport
{
    // The "ab" marker in the prefix is what the reaper filters on.
    public const string BucketNamePrefix = "csharp-sdk-test-ab-";
    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    public static string AccountId => Environment.GetEnvironmentVariable("OSS_TEST_ACCOUNT_ID");

    public static bool Configured =>
        !string.IsNullOrEmpty(AccountId) && !string.IsNullOrEmpty(Utils.Region);

    private static Configuration BaseConfig(string ak, string sk)
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new StaticCredentialsProvider(ak, sk);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.AccountId = AccountId;
        return cfg;
    }

    public static Configuration TestConfig() => BaseConfig(Utils.AccessKeyId, Utils.AccessKeySecret);

    public static AgenticBucketClient GetAgenticClient() => new(TestConfig());

    public static AgenticBucketClient GetInvalidAkClient() => new(BaseConfig("invalid-ak", "invalid-sk"));

    // A non-digit account id passes construction but fails (deferred) at invoke.
    public static AgenticBucketClient GetBadAccountIdClient()
    {
        var cfg = TestConfig();
        cfg.AccountId = "bad-account";
        return new AgenticBucketClient(cfg);
    }

    public static Client GetBucketSpaceClient() => AgenticBucketClient.NewBucketSpaceClient(TestConfig());

    public static string GenBucketName()
    {
        var rnd = new Random();
        var sb = new StringBuilder(6);
        for (var i = 0; i < 6; i++) sb.Append(Letters[rnd.Next(Letters.Length)]);
        return BucketNamePrefix + sb;
    }

    // BuildFullName resolves a short name to the server-side full name
    // "{bucket}-{accountId}-{region}-{suffix}" (suffix "ab-apsr" or "bs-apsr").
    public static string BuildFullName(string bucket, string suffix) =>
        $"{bucket}-{AccountId}-{Utils.Region}-{suffix}";

    // ToShortName strips the resolved tail so a listed name can be passed back to
    // a client that re-expands short names.
    public static string ToShortName(string name, string suffix)
    {
        var tail = $"-{AccountId}-{Utils.Region}-{suffix}";
        return name.EndsWith(tail) ? name.Substring(0, name.Length - tail.Length) : name;
    }

    // DisableAndReap is the shared scenario teardown: disable this run's bucket,
    // then reap buckets left disabled by previous runs.
    public static async Task DisableAndReapAsync(AgenticBucketClient client, string bucket)
    {
        try
        {
            await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
            {
                Bucket = bucket,
                AgenticBucketStatus = new AgenticBucketStatus { Status = "Disabled" }
            });
        }
        catch { /* best effort */ }

        await ReapDisabledAsync(client);
    }

    // ReapDisabledAsync deletes leftover buckets from previous runs that carry our
    // prefix and are already Disabled (Enabled ones may belong to a concurrent
    // run), emptying their bucket spaces first. Best-effort: all errors swallowed.
    public static async Task ReapDisabledAsync(AgenticBucketClient client)
    {
        try
        {
            var paginator = client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
            await foreach (var page in paginator.IterPageAsync())
            {
                if (page.AgenticBuckets == null) continue;
                foreach (var summary in page.AgenticBuckets)
                {
                    var name = summary.Name;
                    if (string.IsNullOrEmpty(name) || !name.StartsWith(BucketNamePrefix)) continue;

                    var bucket = ToShortName(name, "ab-apsr");
                    string status = null;
                    try
                    {
                        var info = await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = bucket });
                        status = info.AgenticBucketInfo?.Status;
                    }
                    catch { continue; }

                    if (status != "Disabled") continue;

                    await ReapBucketSpacesAsync(client, bucket);
                    try { await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = bucket }); }
                    catch { /* not ready / already gone */ }
                }
            }
        }
        catch { /* best effort */ }
    }

    // ReapBucketSpacesAsync empties and deletes every bucket space of a Disabled
    // agentic bucket. Best-effort: errors swallowed.
    private static async Task ReapBucketSpacesAsync(AgenticBucketClient client, string bucket)
    {
        using var bsClient = GetBucketSpaceClient();
        try
        {
            var spacePaginator = client.ListBucketSpacesPaginator(new ListBucketSpacesRequest { Bucket = bucket });
            await foreach (var spacePage in spacePaginator.IterPageAsync())
            {
                if (spacePage.BucketSpaces == null) continue;
                foreach (var space in spacePage.BucketSpaces)
                {
                    if (string.IsNullOrEmpty(space.Name)) continue;
                    var spaceName = ToShortName(space.Name, "bs-apsr");

                    try
                    {
                        var objPaginator = bsClient.ListObjectsV2Paginator(new Models.ListObjectsV2Request { Bucket = spaceName });
                        await foreach (var objPage in objPaginator.IterPageAsync())
                        {
                            if (objPage.Contents == null) continue;
                            foreach (var obj in objPage.Contents)
                            {
                                try { await bsClient.DeleteObjectAsync(new Models.DeleteObjectRequest { Bucket = spaceName, Key = obj.Key }); }
                                catch { /* best effort */ }
                            }
                        }
                    }
                    catch { /* best effort */ }

                    try { await bsClient.DeleteBucketAsync(new Models.DeleteBucketRequest { Bucket = spaceName }); }
                    catch { /* best effort */ }
                }
            }
        }
        catch { /* best effort */ }
    }
}
