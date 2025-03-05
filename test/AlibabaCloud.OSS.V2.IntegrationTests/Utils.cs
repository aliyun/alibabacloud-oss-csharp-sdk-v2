using System.Security.Cryptography;
using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class Utils
{
    private static readonly string _endpoint = null; // your endpoint
    private static readonly string _region = null; // your region
    private static readonly string _accessKeyId = null; // your access key id
    private static readonly string _accessKeySecret = null; // your access key secret
    private static readonly string _ramRoleArn = null; // the ram role arn
    private static readonly string _userId = null; // the uid
    private static readonly string _payerAccessKeyId = null; // payer access key id
    private static readonly string _payerAccessKeySecret = null; // payer access key secret
    private static readonly string _payerUid = null; // payer uid

    private static Client _client;
    private static Client _invalidClient;

    const string BucketNamePrefix = "csharp-sdk-test-bucket-";
    const string ObjectNamePrefix = "csharp-sdk-test-object-";

    public static string Endpoint
        => _endpoint ?? Environment.GetEnvironmentVariable("OSS_TEST_ENDPOINT");

    public static string Region
        => _region ?? Environment.GetEnvironmentVariable("OSS_TEST_REGION");

    public static string AccessKeyId
        => _accessKeyId ?? Environment.GetEnvironmentVariable("OSS_TEST_ACCESS_KEY_ID");

    public static string AccessKeySecret
        => _accessKeySecret ?? Environment.GetEnvironmentVariable("OSS_TEST_ACCESS_KEY_SECRET");

    public static string RamRoleArn
        => _ramRoleArn ?? Environment.GetEnvironmentVariable("OSS_TEST_RAM_ROLE_ARN");

    public static string UserId
        => _userId ?? Environment.GetEnvironmentVariable("OSS_TEST_USER_ID");

    public static string PayerAccessKeyId
        => _payerAccessKeyId ?? Environment.GetEnvironmentVariable("OSS_TEST_PAYER_ACCESS_KEY_ID");

    public static string PayerAccessKeySecret
        => _payerAccessKeySecret ?? Environment.GetEnvironmentVariable("OSS_TEST_PAYER_ACCESS_KEY_SECRET");

    public static string PayerUid
        => _payerUid ?? Environment.GetEnvironmentVariable("OSS_TEST_PAYER_UID");

    public static Client GetDefaultClient()
    {
        if (_client != null) return _client;

        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(AccessKeyId, AccessKeySecret);
        cfg.Region = Region;
        cfg.Endpoint = Endpoint;

        _client = new(cfg);

        return _client;
    }

    public static Client GetClient(string region, string endpoint)
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(AccessKeyId, AccessKeySecret);
        cfg.Region = region;
        cfg.Endpoint = endpoint;

        return new(cfg);
    }

    public static Client GetInvalidAkClient()
    {
        if (_invalidClient != null) return _invalidClient;

        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide("invalid-ak", "invalid-sk");
        cfg.Region = Region;
        cfg.Endpoint = Endpoint;

        _invalidClient = new(cfg);

        return _invalidClient;
    }

    public static string GetTempFileName()
    {
        return $"file-{Guid.NewGuid().ToString()}.tmp";
    }

    public static string GetTempPath()
    {
        //return Path.Join(Path.GetTempPath(), $"csharp-sdk-test-{Guid.NewGuid().ToString()}");
        return $"{Path.GetTempPath()}csharp-sdk-test-{Guid.NewGuid().ToString()}";
    }

    public static string RandomFilePath(string root)
    {
        var uid = Guid.NewGuid().ToString();
        return $"{root}{Path.DirectorySeparatorChar}file-{Guid.NewGuid().ToString()}.tmp";
    }

    public static string NowTimeStamp()
    {
        return Convert.ToString((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
    }

    public static string RandomBucketNamePrefix()
    {
        var uid = Guid.NewGuid().ToString();
        return $"{BucketNamePrefix}{uid.Substring(0, 6)}-{NowTimeStamp()}";
    }

    public static string RandomBucketName(string prefix)
    {
        var uid = Guid.NewGuid().ToString();
        return $"{prefix}-{uid.Substring(0, 6)}";
    }

    public static string RandomBucketName()
    {
        var ran = new Random();
        var n = ran.Next(1000);
        return $"{BucketNamePrefix}{Convert.ToString(n)}-{NowTimeStamp()}";
    }

    public static string RandomObjectName()
    {
        var ran = new Random();
        var n = ran.Next(100);
        return $"{ObjectNamePrefix}{Convert.ToString(n)}-{NowTimeStamp()}";
    }

    public static char GetRandomChar(Random rnd)
    {
        var ret = rnd.Next(122);
        while (ret is < 48 or > 57 and < 65 or > 90 and < 97) ret = rnd.Next(122);
        return (char)ret;
    }

    public static string GetRandomString(int length)
    {
        var rnd = new Random();
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++) sb.Append(GetRandomChar(rnd));
        return sb.ToString();
    }

    public static void WaitFor(int sec)
    {
        Thread.Sleep(sec * 1000);
    }

    public static void CleanBuckets(string prefix)
    {
        Assert.NotEmpty(prefix);
        var client = GetDefaultClient();
        var paginator = client.ListBucketsPaginator(
            new ListBucketsRequest()
            {
                Prefix = prefix
            }
        );

        foreach (var page in paginator.IterPage())
        {
            foreach (var bucket in page.Buckets ?? [])
            {
                try
                {
                    CleanBucket(bucket);
                }
                catch (Exception)
                {
                    //IGNORE
                }
            }
        }
    }

    private static void CleanBucket(BucketProperties bucket)
    {
        var client = GetDefaultClient();

        if (!string.Equals(Region, bucket.Region))
        {
            var endpoint = bucket.ExtranetEndpoint;

            if (Endpoint != null)
            {
                if (Endpoint.Contains("-internal.")) endpoint = bucket.IntranetEndpoint;

                if (Endpoint.StartsWith("http://")) endpoint = $"http://{endpoint}";
            }

            client = GetClient(bucket.Region, endpoint);
        }

        // list all objects & delete
        var paginator = client.ListObjectVersionsPaginator(
            new ListObjectVersionsRequest(
            )
            {
                Bucket = bucket.Name,
                MaxKeys = 1000
            }
        );

        foreach (var page in paginator.IterPage())
        {
            var obj = new List<Models.DeleteObject>();
            foreach (var version in page.Versions ?? [])
            {
                obj.Add(new DeleteObject()
                {
                    Key = version.Key,
                    VersionId = version.VersionId
                });
            }
            Assert.NotNull(obj);
            if (obj.Count > 0)
            {
                client.DeleteMultipleObjectsAsync(
                    new Models.DeleteMultipleObjectsRequest()
                    {
                        Bucket = bucket.Name,
                        Objects = obj
                    }
                ).GetAwaiter().GetResult();
            }
        }

        var mpPaginators = client.ListMultipartUploadsPaginator(
            new ListMultipartUploadsRequest()
            {
                Bucket = bucket.Name
            });
        foreach (var page in mpPaginators.IterPage())
        {
            foreach (var upload in page.Uploads)
            {
                client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest()
                {
                    Bucket = bucket.Name,
                    UploadId = upload.UploadId,
                    Key = upload.Key,
                }).GetAwaiter().GetResult();
            }
        }

        var result = client.DeleteBucketAsync(
            new DeleteBucketRequest()
            {
                Bucket = bucket.Name
            }
        ).GetAwaiter().GetResult();
        Assert.NotNull(result);
    }

    public static void CleanPath(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void PrepareSampleFile(string filePath, int numberOfKbs)
    {
        if (File.Exists(filePath)) return;

        using var file = new StreamWriter(filePath);
        for (var i = 0; i < numberOfKbs; i++)
        {
            file.WriteLine(GenerateOneKb());
        }
        file.Flush();
    }

    private const string AvailableCharacters = "01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int OneKbCount = 1024;
    public static string GenerateOneKb()
    {
        var r = new Random();
        var sb = new StringBuilder();
        var i = 0;
        while (i++ < OneKbCount)
        {
            var pos = r.Next(AvailableCharacters.Length);
            sb.Append(AvailableCharacters[pos]);
        }
        return sb.ToString();
    }

    public static string ComputeContentMd5(string inputFile)
    {
        using (Stream inputStream = File.OpenRead(inputFile))
        {
            using (var md5 = MD5.Create())
            {
                // Compute hash data of the input stream.

                var data = md5.ComputeHash(inputStream);
                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }

    public static string ComputeContentMd5(Stream inputStream)
    {
        using (var md5 = MD5.Create())
        {
            var data = md5.ComputeHash(inputStream);
            var sBuilder = new StringBuilder();
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}

