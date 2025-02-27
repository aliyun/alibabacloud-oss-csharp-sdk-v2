using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelRegionTest {
    [Fact]
    public void TestDescribeRegionsRequest() {
        var request = new DescribeRegionsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Regions);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new DescribeRegionsRequest {
            Regions = "oss-cn-hangzhou",
        };
        Assert.Single(request.Parameters);
        Assert.Equal("oss-cn-hangzhou", request.Parameters["regions"]);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);

        Assert.NotNull(input.Parameters);
        Assert.Equal("oss-cn-hangzhou", input.Parameters["regions"]);
    }

    [Fact]
    public void TestDescribeRegionsResult() {
        var result = new DescribeRegionsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<RegionInfoList>
</RegionInfoList>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.Empty(result.RegionInfoList.RegionInfos);

        xml = """
<?xml version="1.0" encoding="utf-8"?>
<RegionInfoList>
  <RegionInfo>
     <Region>oss-cn-hangzhou</Region>
     <InternetEndpoint>oss-cn-hangzhou.aliyuncs.com</InternetEndpoint>
     <InternalEndpoint>oss-cn-hangzhou-internal.aliyuncs.com</InternalEndpoint>
     <AccelerateEndpoint>oss-accelerate.aliyuncs.com</AccelerateEndpoint>  
  </RegionInfo>
  <RegionInfo>
     <Region>oss-cn-shanghai</Region>
     <InternetEndpoint>oss-cn-shanghai.aliyuncs.com</InternetEndpoint>
     <InternalEndpoint>oss-cn-shanghai-internal.aliyuncs.com</InternalEndpoint>
     <AccelerateEndpoint>oss-accelerate.aliyuncs.com</AccelerateEndpoint>  
  </RegionInfo>
</RegionInfoList>
""";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.Equal("oss-cn-hangzhou", result.RegionInfoList.RegionInfos[0].Region);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", result.RegionInfoList.RegionInfos[0].InternetEndpoint);
        Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", result.RegionInfoList.RegionInfos[0].InternalEndpoint);
        Assert.Equal("oss-accelerate.aliyuncs.com", result.RegionInfoList.RegionInfos[0].AccelerateEndpoint);

        Assert.Equal("oss-cn-shanghai", result.RegionInfoList.RegionInfos[1].Region);
        Assert.Equal("oss-cn-shanghai.aliyuncs.com", result.RegionInfoList.RegionInfos[1].InternetEndpoint);
        Assert.Equal("oss-cn-shanghai-internal.aliyuncs.com", result.RegionInfoList.RegionInfos[1].InternalEndpoint);
        Assert.Equal("oss-accelerate.aliyuncs.com", result.RegionInfoList.RegionInfos[1].AccelerateEndpoint);
    }
}