using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.PostObject
{
    public class Program
    {

        public class Options
        {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("bucket", Required = true, HelpText = "The `name` of the bucket.")]
            public string? Bucket { get; set; }

            [Option("key", Required = true, HelpText = "The `name` of the object.")]
            public string? Key { get; set; }
        }

        static string ToHexString(byte[] data, bool lowercase)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++) sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
            return sb.ToString();
        }

        static string quote(string value)
        {
            return $"\"{value}\"";
        }

        public static async Task Main(string[] args)
        {

            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Errors.Any())
            {
                Environment.Exit(1);
            }
            var option = parserResult.Value;

            // Specify the region and other parameters.
            var region = option.Region!;
            var bucket = option.Bucket!;
            var key = option.Key!;
            var product = "oss";

            // loading credentials values from the environment variables
            var credentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
            var credentials = credentialsProvider.GetCredentials();

            var content = "hi oss";

            // build policy
            var utcTime = DateTime.UtcNow;
            var date = utcTime.ToUniversalTime().ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var dateTime = utcTime.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
            var expiration = utcTime.AddHours(1);
            var credentialInfo = $"{credentials.AccessKeyId}/{date}/{region}/{product}/aliyun_v4_request";
            var policyMap = new Dictionary<string, Object>()
            {
                { "expiration",
                    expiration.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.000'Z'", CultureInfo.InvariantCulture)
                },
                { "conditions", new Object[]{
                     new Dictionary<string, string>() {{ "bucket", bucket } },
                     new Dictionary<string, string>() {{ "x-oss-signature-version", "OSS4-HMAC-SHA256" } },
                     new Dictionary<string, string>() {{ "x-oss-credential", credentialInfo } },
                     new Dictionary<string, string>() {{ "x-oss-date", dateTime } },
                     //other condition
                     new Object[]{"content-length-range", 1, 1024 },
                     //new Object[]{"eq", "$success_action_status", "201"},
                     //new Object[]{"starts-with", "$key", "user/eric/"},
                     //new Object[]{"in", "$content-type", new string[]{"image/jpg", "image/png"}},
                     //new Object[]{ "not-in", "$cache-control", new string[]{ "no-cache" } },
                    }
                },
            };

            var policy = JsonSerializer.Serialize(policyMap);

            // sign policy
            var stringToSign = Convert.ToBase64String(Encoding.UTF8.GetBytes(policy));

            // signing key
            using var kha = new HMACSHA256();

            var ksecret = Encoding.UTF8.GetBytes("aliyun_v4" + credentials.AccessKeySecret);

            kha.Key = ksecret;
            var hashDate = kha.ComputeHash(Encoding.UTF8.GetBytes(date));

            kha.Key = hashDate;
            var hashRegion = kha.ComputeHash(Encoding.UTF8.GetBytes(region));

            kha.Key = hashRegion;
            var hashProduct = kha.ComputeHash(Encoding.UTF8.GetBytes(product));

            kha.Key = hashProduct;
            var signingKey = kha.ComputeHash(Encoding.UTF8.GetBytes("aliyun_v4_request"));

            // Signature
            kha.Key = signingKey;
            var signature = ToHexString(kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)), true);

            // Multipar form
            using var formData = new MultipartFormDataContent();
            // trim quote
            var boundary = formData.Headers.ContentType!.Parameters.ElementAt(0).Value!;
            formData.Headers.ContentType.Parameters.ElementAt(0).Value = boundary.Trim('"');

            // object info, key & metadata
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(key)), quote("key"));
            // meta-data
            //formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(value)), quote("x-oss-"));
            // policy
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(stringToSign)), quote("policy"));
            // Signature
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("OSS4-HMAC-SHA256")), quote("x-oss-signature-version"));
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(credentialInfo)), quote("x-oss-credential"));
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(dateTime)), quote("x-oss-date"));
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(signature)), quote("x-oss-signature"));

            // Data
            formData.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(content)), quote("file"));

            // Send request
            using var hc = new HttpClient();

            var result = await hc.PostAsync($"http://{bucket}.oss-{region}.aliyuncs.com/", formData);

            Console.WriteLine("PostObject done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + String.Join(",", x.Value.ToList())));
        }
    }
}
