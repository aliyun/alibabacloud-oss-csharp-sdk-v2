namespace AlibabaCloud.OSS.V2.UnitTests;

public class ExceptionTest
{
    [Fact]
    public void TestServiceException()
    {
        var exception = new ServiceException(0, null);
        Assert.Equal(0, exception.StatusCode);
        Assert.Equal("", exception.ErrorCode);
        Assert.Equal("", exception.ErrorMessage);
        Assert.Equal("", exception.Ec);
        Assert.Equal("", exception.RequestTarget);
        Assert.Equal("", exception.Snapshot);
        Assert.Empty(exception.ErrorFields);
        Assert.Empty(exception.Headers);

        var msg = "AlibabaCloud.OSS.V2.ServiceException: Error returned by Service.\n" +
                  "Http Status Code: 0\n" +
                  "Error Code: \n" +
                  "Request Id: \n" +
                  "Message: \n" +
                  "EC: \n" +
                  "Timestamp: \n" +
                  "Request Endpoint: ";
        Assert.Equal(msg, $"{exception}");

        // has details
        var details = new Dictionary<string, string>() {
            { "Code", "InternalError" },
            { "Message", "Please contact the server administrator, oss@service.aliyun.com" },
            { "RequestId", "id-1234" },
            { "Ec", "0002-00000902" },
            { "RequestTarget", "POST https://bucket.oss-cn-shenzhen.aliyuncs.com" },
            { "TimeStamp", "request time 123"},
            { "Snapshot", "xml values"}
        };
        exception = new ServiceException(500, details);
        Assert.Equal(500, exception.StatusCode);
        Assert.Equal("InternalError", exception.ErrorCode);
        Assert.Equal("Please contact the server administrator, oss@service.aliyun.com", exception.ErrorMessage);
        Assert.Equal("id-1234", exception.RequestId);
        Assert.Equal("0002-00000902", exception.Ec);
        Assert.Equal("POST https://bucket.oss-cn-shenzhen.aliyuncs.com", exception.RequestTarget);
        Assert.Equal("request time 123", exception.TimeStamp);
        Assert.Equal("xml values", exception.Snapshot);
        Assert.Empty(exception.ErrorFields);
        Assert.Empty(exception.Headers);

        msg = "AlibabaCloud.OSS.V2.ServiceException: Error returned by Service.\n" +
            "Http Status Code: 500\n" +
            "Error Code: InternalError\n" +
            "Request Id: id-1234\n" +
            "Message: Please contact the server administrator, oss@service.aliyun.com\n" +
            "EC: 0002-00000902\n" +
            "Timestamp: request time 123\n" +
            "Request Endpoint: POST https://bucket.oss-cn-shenzhen.aliyuncs.com";
        Assert.Equal(msg, $"{exception}");

        // has ErrorFields and headers
        var errorFields = new Dictionary<string, string>() {
            { "Code", "InternalError" },
            { "Message", "Please contact the server administrator, oss@service.aliyun.com" },
            { "RequestId", "id-1234" },
            { "EC", "0002-00000902" },
            { "HostId", "oss-cn-hangzhou.aliyuncs.com" },
        };

        var headers = new Dictionary<string, string>() {
            { "x-oss-request-id", "header-request-id-1" }
        };
        exception = new ServiceException(504, details, errorFields, headers);
        Assert.Equal(504, exception.StatusCode);
        Assert.Equal("InternalError", exception.ErrorCode);
        Assert.Equal("Please contact the server administrator, oss@service.aliyun.com", exception.ErrorMessage);
        Assert.Equal("id-1234", exception.RequestId);
        Assert.Equal("0002-00000902", exception.Ec);
        Assert.Equal("POST https://bucket.oss-cn-shenzhen.aliyuncs.com", exception.RequestTarget);
        Assert.Equal("xml values", exception.Snapshot);
        Assert.NotEmpty(exception.ErrorFields);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", exception.ErrorFields["HostId"]);
        Assert.NotEmpty(exception.Headers);
        Assert.Equal("header-request-id-1", exception.Headers["x-oss-request-id"]);
    }

    [Fact]
    public void TestOperationException()
    {
        var exception = new OperationException("nop");
        Assert.NotNull(exception);
        Assert.Equal("operation error nop: ", exception.Message);

        var err = new Exception("got error");
        exception = new OperationException("PutObject", err);
        Assert.NotNull(exception);
        Assert.Equal("operation error PutObject: System.Exception: got error", exception.Message);
        Assert.Equal(err, exception.InnerException);
    }

}
