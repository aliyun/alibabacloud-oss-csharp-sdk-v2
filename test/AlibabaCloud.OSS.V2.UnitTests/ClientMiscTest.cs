using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;
using System.Net;
using System.Text;

namespace AlibabaCloud.OSS.V2.UnitTests;
public class ClientMiscTest {

    [Fact]
    public async Task TestAutoDetectMimeTypeEnable() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new Client(config);

        var bucketName = "bucket-123";
        const string content = "hello world";

        // put object
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }
        ];
        var putResult = await client.PutObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("text/plain", mockHandler.LastRequest.Content.Headers.ContentType.ToString());

        // append object
        mockHandler.Clear();
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(""),
        };
        response.Headers.Add("x-oss-next-append-position", "11");
        mockHandler.Responses = [response];
        var appendResult = await client.AppendObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.json",
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("application/json", mockHandler.LastRequest.Content.Headers.ContentType.ToString());

        // init object
        mockHandler.Clear();
        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<InitiateMultipartUploadResult/>"),
        };
        mockHandler.Responses = [response];
        var initResult = await client.InitiateMultipartUploadAsync(
            new() {
                Bucket = bucketName,
                Key = "test.jpg",
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("image/jpeg", mockHandler.LastRequest.Content.Headers.ContentType.ToString());

        // init object
        mockHandler.Clear();
        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<InitiateMultipartUploadResult/>"),
        };
        mockHandler.Responses = [response];
        initResult = await client.InitiateMultipartUploadAsync(
            new() {
                Bucket = bucketName,
                Key = "test.jpg",
                DisableAutoDetectMimeType = true
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Null(mockHandler.LastRequest.Content.Headers.ContentType);
    }

    [Fact]
    public async Task TestAutoDetectMimeTypeDisable() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new Client(config, x => { x.FeatureFlags = x.FeatureFlags & ~FeatureFlagsType.AutoDetectMimeType; });

        var bucketName = "bucket-123";
        const string content = "hello world";

        // put object
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }
        ];
        var putResult = await client.PutObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Null(mockHandler.LastRequest.Content.Headers.ContentType);

        // append object
        mockHandler.Clear();
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(""),
        };
        response.Headers.Add("x-oss-next-append-position", "11");
        mockHandler.Responses = [response];
        var appendResult = await client.AppendObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.json",
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Null(mockHandler.LastRequest.Content.Headers.ContentType);

        // init object
        mockHandler.Clear();
        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<InitiateMultipartUploadResult/>"),
        };
        mockHandler.Responses = [response];
        var initResult = await client.InitiateMultipartUploadAsync(
            new() {
                Bucket = bucketName,
                Key = "test.jpg",
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Null(mockHandler.LastRequest.Content.Headers.ContentType);
    }

    [Fact]
    public async Task TestAutoDetectMimeTypeEnableWithContentTypeHeader() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new Client(config);

        var bucketName = "bucket-123";
        const string content = "hello world";

        // put object
        mockHandler.Clear();
        mockHandler.Responses = [
            new() {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent("")
            }
        ];
        var putResult = await client.PutObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
                ContentType = "text/test-1",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("text/test-1", mockHandler.LastRequest.Content.Headers.ContentType.ToString());

        // append object
        mockHandler.Clear();
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(""),
        };
        response.Headers.Add("x-oss-next-append-position", "11");
        mockHandler.Responses = [response];
        var appendResult = await client.AppendObjectAsync(
            new() {
                Bucket = bucketName,
                Key = "test.json",
                Position = 0,
                ContentType = "text/test-2",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("text/test-2", mockHandler.LastRequest.Content.Headers.ContentType.ToString());

        // init object
        mockHandler.Clear();
        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<InitiateMultipartUploadResult/>"),
        };
        mockHandler.Responses = [response];
        var initResult = await client.InitiateMultipartUploadAsync(
            new() {
                Bucket = bucketName,
                Key = "test.jpg",
                ContentType = "text/test-3",
            }
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal("text/test-3", mockHandler.LastRequest.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task TestGetObjectToFileAsyncCrc64() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new Client(config);

        var bucketName = "bucket-123";
        const string content = "test";
        const string contentCrc = "18020588380933092773";

        var savePath = Utils.GetTempFileName();

        // get object to file, has response crc
        mockHandler.Clear();
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        response.Headers.Add("x-oss-hash-crc64ecma", contentCrc);
        mockHandler.Responses = [response];
        var getResult = await client.GetObjectToFileAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
            },
            savePath
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);
        var gotContent = File.ReadAllBytes(savePath);
        Assert.Equal(Encoding.UTF8.GetBytes(content), gotContent);
        Assert.Equal(contentCrc, getResult.HashCrc64);
        File.Delete(savePath);

        // get object to file, without response crc
        mockHandler.Clear();
        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        mockHandler.Responses = [response];
        getResult = await client.GetObjectToFileAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
            },
            savePath
        );
        Assert.NotNull(mockHandler.LastRequest);
        gotContent = File.ReadAllBytes(savePath);
        Assert.Equal(Encoding.UTF8.GetBytes(content), gotContent);
        Assert.Null(getResult.HashCrc64);
        File.Delete(savePath);

        // get object with 1 error crc
        mockHandler.Clear();
        var responseFail = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        responseFail.Headers.Add("x-oss-hash-crc64ecma", "1234");

        response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        response.Headers.Add("x-oss-hash-crc64ecma", contentCrc);
        mockHandler.Responses = [responseFail, response];
        getResult = await client.GetObjectToFileAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
            },
            savePath
        );
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal(2, mockHandler.Requests.Count);
        gotContent = File.ReadAllBytes(savePath);
        Assert.Equal(Encoding.UTF8.GetBytes(content), gotContent);
        Assert.Equal(contentCrc, getResult.HashCrc64);
        File.Delete(savePath);

        // get object with 3 error crc
        mockHandler.Clear();
        var responseFail1 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        responseFail1.Headers.Add("x-oss-hash-crc64ecma", "1234");

        var responseFail2 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        responseFail2.Headers.Add("x-oss-hash-crc64ecma", "1234");

        var responseFail3 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        responseFail3.Headers.Add("x-oss-hash-crc64ecma", "1234");

        try {
            mockHandler.Responses = [responseFail1, responseFail2, responseFail3];
            getResult = await client.GetObjectToFileAsync(
                new() {
                    Bucket = bucketName,
                    Key = "test.txt",
                },
                savePath
            );
            Assert.Fail("should not here");
        }
        catch (InconsistentException) {
            Assert.True(true);
        }
        File.Delete(savePath);

        // disable crc
        var config1 = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
            DisableDownloadCrc64Check = true
        };
        using var client1 = new Client(config1);
        mockHandler.Clear();
        responseFail1 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        responseFail1.Headers.Add("x-oss-hash-crc64ecma", "1234");
        mockHandler.Responses = [responseFail1];
        getResult = await client1.GetObjectToFileAsync(
            new() {
                Bucket = bucketName,
                Key = "test.txt",
            },
            savePath
        );
        Assert.NotNull(mockHandler.LastRequest);
        gotContent = File.ReadAllBytes(savePath);
        Assert.Equal(Encoding.UTF8.GetBytes(content), gotContent);
        Assert.Equal("1234", getResult.HashCrc64);
        File.Delete(savePath);
    }

    [Fact]
    public async Task TestGetObjectToFileAsyncCancellationToken() {
        var mockHandler = new MockHttpMessageHandler();
        var config = new Configuration() {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mockHandler),
        };
        var client = new Client(config);

        var bucketName = "bucket-123";
        const string content = "test";
        const string contentCrc = "18020588380933092773";

        var savePath = Utils.GetTempFileName();

        // get object to file, with canceled CancellationToken
        mockHandler.Clear();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var response = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content),
        };
        response.Headers.Add("x-oss-hash-crc64ecma", contentCrc);
        mockHandler.Responses = [response];

        try {
            var getResult = await client.GetObjectToFileAsync(
                new() {
                    Bucket = bucketName,
                    Key = "test.txt",
                },
                savePath,
                null,
                cts.Token
            );
            Assert.Fail("should not here");
        }
        catch (OperationCanceledException) {
            Assert.True(true);
        }
        File.Delete(savePath);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Single(mockHandler.Requests);

        // get object to file, with readtimeout CancellationToken
        mockHandler.Clear();
        var responseTimeout1 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(
                new TimeoutReadStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(content)), TimeSpan.FromSeconds(4))),
        };

        var responseTimeout2 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(
                new TimeoutReadStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(content)), TimeSpan.FromSeconds(4))),
        };

        var responseTimeout3 = new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(
                new TimeoutReadStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(content)), TimeSpan.FromSeconds(4))),
        };

        mockHandler.Responses = [responseTimeout1, responseTimeout2, responseTimeout3];

        try {
            var getResult = await client.GetObjectToFileAsync(
                new() {
                    Bucket = bucketName,
                    Key = "test.txt",
                },
                savePath,
                new OperationOptions() { ReadWriteTimeout = TimeSpan.FromSeconds(2) }
            );
            Assert.Fail("should not here");
        }
        catch (RequestTimeoutException) {
            Assert.True(true);
        }
        File.Delete(savePath);
        Assert.NotNull(mockHandler.LastRequest);
        Assert.Equal(3, mockHandler.Requests.Count);

        // error hanppens when call GetObjectAsync
        mockHandler.Clear();
        try {
            var getResult = await client.GetObjectToFileAsync(
                new() {
                    Bucket = bucketName,
                    Key = "",
                },
                savePath,
                new OperationOptions() { ReadWriteTimeout = TimeSpan.FromSeconds(2) }
            );
            Assert.Fail("should not here");
        }
        catch (ArgumentException) {
            Assert.True(true);
        }
        File.Delete(savePath);
        Assert.Null(mockHandler.LastRequest);
    }
}
