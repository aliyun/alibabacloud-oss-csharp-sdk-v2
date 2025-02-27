using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientRegionTest : IDisposable {
    private readonly string BucketNamePrefix;

    public void Dispose() {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientRegionTest() {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestDescribeRegions() {
        var client = Utils.GetDefaultClient();

        // without region
        var result = await client.DescribeRegionsAsync(new DescribeRegionsRequest());
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.NotEmpty(result.RegionInfoList.RegionInfos);
        var found = false;
        foreach (var region in result.RegionInfoList.RegionInfos) {
            if (region.Region == "oss-cn-hangzhou") {
                found = true;
                Assert.Equal("oss-cn-hangzhou.aliyuncs.com", region.InternetEndpoint);
                Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", region.InternalEndpoint);
                Assert.Equal("oss-accelerate.aliyuncs.com", region.AccelerateEndpoint);
            }
        }
        Assert.True(found);

        // with region
        result = await client.DescribeRegionsAsync(new DescribeRegionsRequest() {
            Regions = "oss-cn-shenzhen"
        });
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.Single(result.RegionInfoList.RegionInfos);
        var info = result.RegionInfoList.RegionInfos;
        Assert.Equal("oss-cn-shenzhen.aliyuncs.com", info[0].InternetEndpoint);
        Assert.Equal("oss-cn-shenzhen-internal.aliyuncs.com", info[0].InternalEndpoint);
        Assert.Equal("oss-accelerate.aliyuncs.com", info[0].AccelerateEndpoint);
    }

    [Fact]
    public async Task TestDescribeRegionsFail() {
        var invClient = Utils.GetInvalidAkClient();

        try {
            await invClient.DescribeRegionsAsync(new DescribeRegionsRequest());
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error DescribeRegions", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?regions", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }
}
