using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.PresignPutObject
{
    public class Program
    {

        public class Options
        {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }

            [Option("bucket", Required = true, HelpText = "The `name` of the bucket.")]
            public string? Bucket { get; set; }

            [Option("key", Required = true, HelpText = "The `name` of the object.")]
            public string? Key { get; set; }
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
            var region = option.Region;
            var bucket = option.Bucket;
            var endpoint = option.Endpoint;
            var key = option.Key;

            // Using the SDK's default configuration
            // loading credentials values from the environment variables
            var cfg = OSS.Configuration.LoadDefault();
            cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
            cfg.Region = region;

            if (endpoint != null)
            {
                cfg.Endpoint = endpoint;
            }

            using var client = new OSS.Client(cfg);

            var result = client.Presign(new OSS.Models.PutObjectRequest()
            {
                Bucket = bucket,
                Key = key,
            });

            const string content = "hi oss!";
            using var hc = new HttpClient();
            var httpResult = await hc.PutAsync(result.Url, new ByteArrayContent(Encoding.UTF8.GetBytes(content)));

            Console.WriteLine("PutObject done");
            Console.WriteLine($"StatusCode: {httpResult.StatusCode}");

            /*
            //with props
            result = client.Presign(
                new PutObjectRequest()
                {
                    Bucket = bucket,
                    Key = key,
                    StorageClass = "IA",
                    Acl = "private",
                    ContentType = "text/txt",
                    Metadata = new Dictionary<string, string>() {
                        { "key1", "value1" },
                        { "key2", "value2" }
                    },
                    Tagging = "tag-key1=val1",
                }
            );

            var content1 = "hello world";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, new Uri(result.Url));
            requestMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content1));
            foreach (var item in result.SignedHeaders!)
            {
                switch (item.Key.ToLower())
                {
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
            */
        }
    }
}
