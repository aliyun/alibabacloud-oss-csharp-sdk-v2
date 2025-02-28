using AlibabaCloud.OSS.V2.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientPresignerTest : IDisposable {
    private readonly string BucketNamePrefix;

    public void Dispose() {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientPresignerTest() {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestPresignGetAndPutObject() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new() {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world, hi oss!";

        var preResult = client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        using var hc = new HttpClient();
        var httpResult = await hc.PutAsync(preResult.Url, new ByteArrayContent(Encoding.UTF8.GetBytes(content)));
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        preResult = client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        httpResult = await hc.GetAsync(preResult.Url);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
        var stream = await httpResult.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);

        // full props
        preResult = client.Presign(
            new PutObjectRequest() {
                Bucket = bucketName,
                Key = objectName,
                StorageClass = "IA",
                Acl = "private",
                ContentDisposition = "1.txt",
                CacheControl = "no-cache",
                ContentEncoding = "deflate",
                Expires = "Wed, 21 Oct 2015 07:28:00 GMT",
                ContentType = "text/txt",
                ContentMd5 = "XrY7u+Ae7tCTyyK7j1rNww==",
                Metadata = new Dictionary<string, string>() {
                            { "key1", "value1" },
                            { "key2", "value2" }
                },
                Tagging = "tag-key1=val1",
            }
        );
        Assert.NotNull(preResult);
        Assert.NotEmpty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        var content1 = "hello world";
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, new Uri(preResult.Url));
        requestMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content1));
        foreach (var item in preResult.SignedHeaders) {
            switch (item.Key.ToLower()) {
                case "content-disposition":
                    requestMessage.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(item.Value);
                    break;
                case "content-encoding":
                    requestMessage.Content.Headers.ContentEncoding.Add(item.Value);
                    break;
                case "content-language":
                    requestMessage.Content.Headers.ContentLanguage.Add(item.Value);
                    break;
                case "content-type":
                    requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(item.Value);
                    break;
                case "content-md5":
                    requestMessage.Content.Headers.ContentMD5 = Convert.FromBase64String(item.Value);
                    break;
                case "content-length":
                    requestMessage.Content.Headers.ContentLength = Convert.ToInt64(item.Value);
                    break;
                case "expires":
                    if (DateTime.TryParse(
                            item.Value,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var expires
                        ))
                        requestMessage.Content.Headers.Expires = expires;
                    break;
                default:
                    requestMessage.Headers.Add(item.Key, item.Value);
                    break;
            }
        }

        httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        preResult = client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.NotNull(preResult.Expiration);
        httpResult = await hc.GetAsync(preResult.Url);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
        stream = await httpResult.Content.ReadAsStreamAsync();
        using var reader1 = new StreamReader(stream);
        got = await reader1.ReadToEndAsync();
        Assert.Equal(content1, got);
        Assert.Equal("IA", httpResult.Headers.GetValues("x-oss-storage-class").ToList()[0]);
        Assert.Equal("text/txt", httpResult.Content.Headers.ContentType.ToString());

        // head object
        preResult = client.Presign(
            new HeadObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("HEAD", preResult.Method);
        Assert.NotNull(preResult.Expiration);
        requestMessage = new HttpRequestMessage(HttpMethod.Head, new Uri(preResult.Url));
        httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
        Assert.Equal("IA", httpResult.Headers.GetValues("x-oss-storage-class").ToList()[0]);
        Assert.Equal("text/txt", httpResult.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task TestPresignGetAndPutObjectV1() {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = "cn-shenzhen";
        cfg.SignatureVersion = "v1";

        using var client = new Client(cfg);

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new() {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world, hi oss!";

        var preResult = client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        using var hc = new HttpClient();
        var httpResult = await hc.PutAsync(preResult.Url, new ByteArrayContent(Encoding.UTF8.GetBytes(content)));
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        preResult = client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        httpResult = await hc.GetAsync(preResult.Url);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
        var stream = await httpResult.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }

    [Fact]
    public async Task TestPresignMulitipartObject() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new() {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        using var hc = new HttpClient();

        // only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world, hi oss!";

        // init
        var preResult = client.Presign(
            new InitiateMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(preResult.Url));
        var httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        var initResult = await client.InitiateMultipartUploadAsync(new() {
            Bucket = bucketName,
            Key = objectName
        });
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);
        Assert.NotNull(initResult.RequestId);
        Assert.Equal("url", initResult.EncodingType);

        // upload part
        preResult = client.Presign(
            new UploadPartRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId,
                PartNumber = 1
            }
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        requestMessage = new HttpRequestMessage(HttpMethod.Put, new Uri(preResult.Url));
        requestMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        // complete
        preResult = client.Presign(
            new CompleteMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId,
                CompleteAll = "yes"
            }
        );
        Assert.NotNull(preResult);
        Assert.NotEmpty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(preResult.Url));
        requestMessage.Headers.Add("x-oss-complete-all", "yes");
        httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        // get object
        var getObjectResult = await client.GetObjectAsync(
            new() {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getObjectResult);
        Assert.Equal(200, getObjectResult.StatusCode);
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }

    [Fact]
    public async Task TestPresignAbortMultipartUpload() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new() {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var objectName = Utils.RandomObjectName();
        var initResult = await client.InitiateMultipartUploadAsync(new() {
            Bucket = bucketName,
            Key = objectName
        });
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);
        Assert.NotNull(initResult.RequestId);
        Assert.Equal("url", initResult.EncodingType);

        var preResult = client.Presign(
            new AbortMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId,
            });

        using var hc = new HttpClient();

        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, new Uri(preResult.Url));
        var httpResult = await hc.SendAsync(requestMessage);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
    }

    [Fact]
    public void TestPresignFail() {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("", "");
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;

        using var client = new Client(cfg);
        var bucketName = "bucket-name";
        var objectName = "object-name";

        // get object
        try {
            client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // put object
        try {
            client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // head object
        try {
            client.Presign(
            new HeadObjectRequest {
                Bucket = bucketName,
                Key = objectName
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // InitiateMultipartUpload
        try {
            client.Presign(
            new InitiateMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // UploadPart
        try {
            client.Presign(
            new UploadPartRequest {
                Bucket = bucketName,
                Key = objectName,
                PartNumber = 1,
                UploadId = "upload-id"
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // CompleteMultipartUpload
        try {
            client.Presign(
            new CompleteMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
                CompleteAll = "yes"
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }

        // AbortMultipartUpload
        try {
            client.Presign(
            new AbortMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
            });
        }
        catch (Exception e) {
            Assert.Contains("Credentials is null or empty", e.Message);
        }
    }

    [Fact]
    public void TestPresignWithExpirationTimeV4() {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("ak", "sk");
        cfg.Region = "cn-shenzhen";

        using var client = new Client(cfg);

        var bucketName = "bucket-123";
        var objectName = "object-123";

        var expiration = DateTime.UtcNow.AddHours(-1);

        // put object
        var preResult = client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        // get object
        preResult = client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        // head object
        preResult = client.Presign(
            new HeadObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("HEAD", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        // init
        preResult = client.Presign(
            new InitiateMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        // upload part
        preResult = client.Presign(
            new UploadPartRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
                PartNumber = 1
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        // complete
        preResult = client.Presign(
            new CompleteMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
                CompleteAll = "yes"
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.NotEmpty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);

        preResult = client.Presign(
            new AbortMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("DELETE", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains("x-oss-expires=-", preResult.Url);
    }

    [Fact]
    public void TestPresignWithExpirationTimeV1() {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("ak", "sk");
        cfg.Region = "cn-shenzhen";
        cfg.SignatureVersion = "v1";

        using var client = new Client(cfg);

        var bucketName = "bucket-123";
        var objectName = "object-123";

        var expiration = DateTime.UtcNow.AddHours(-1);
        var expires = FormatUnixTime(expiration);

        // put object
        var preResult = client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        // get object
        preResult = client.Presign(
            new GetObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        // head object
        preResult = client.Presign(
            new HeadObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("HEAD", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        // init
        preResult = client.Presign(
            new InitiateMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        // upload part
        preResult = client.Presign(
            new UploadPartRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
                PartNumber = 1
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        // complete
        preResult = client.Presign(
            new CompleteMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
                CompleteAll = "yes"
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.NotEmpty(preResult.SignedHeaders);
        Assert.Equal("POST", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);

        preResult = client.Presign(
            new AbortMultipartUploadRequest {
                Bucket = bucketName,
                Key = objectName,
                UploadId = "upload-id",
            },
            expiration
        );
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("DELETE", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);
    }

    [Fact]
    public void TestPresignWithPresignExpirationException() {

        // v1 supports > 7 days Expiration
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("ak", "sk");
        cfg.Region = "cn-shenzhen";
        cfg.SignatureVersion = "v1";

        using var client = new Client(cfg);

        var bucketName = "bucket-123";
        var objectName = "object-123";

        var expiration = DateTime.UtcNow.AddDays(8);
        var expires = FormatUnixTime(expiration);

        // put object
        var preResult = client.Presign(
            new PutObjectRequest {
                Bucket = bucketName,
                Key = objectName
            },
            expiration
        );

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.Equal(expiration, preResult.Expiration);
        Assert.Contains($"Expires={expires}", preResult.Url);


        // v4 does not support > 7 days Expiration
        cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("ak", "sk");
        cfg.Region = "cn-shenzhen";
        cfg.SignatureVersion = "v4";

        using var client1 = new Client(cfg);

        expiration = DateTime.UtcNow.AddDays(8);

        // put object
        try {
            preResult = client1.Presign(
                new PutObjectRequest {
                    Bucket = bucketName,
                    Key = objectName
                },
                expiration
            );
            Assert.Fail("should not here");
        }
        catch (PresignExpirationException e) {
            Assert.Contains("Expires should be not greater than 604800 s (seven days)", e.ToString());
        }
    }

    private static string FormatUnixTime(DateTime time) {
        const long ticksOf1970 = 621355968000000000;
        return ((time.ToUniversalTime().Ticks - ticksOf1970) / 10000000L).ToString(CultureInfo.InvariantCulture);
    }
}
