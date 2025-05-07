using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.AppendObject
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

            var content1 = "hi,";
            var content2 = "oss!";

            var result1 = await client.AppendObjectAsync(new OSS.Models.AppendObjectRequest()
            {
                Bucket = bucket,
                Key = key,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content1))
            });

            var result2 = await client.AppendObjectAsync(new OSS.Models.AppendObjectRequest()
            {
                Bucket = bucket,
                Key = key,
                Position = result1.NextAppendPosition,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content2))
            });

            Console.WriteLine("AppendObject done");
            Console.WriteLine($"StatusCode: {result2.StatusCode}");
            Console.WriteLine($"RequestId: {result2.RequestId}");
            Console.WriteLine("Response Headers:");
            result2.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
            Console.WriteLine($"NextAppendPosition: {result2.NextAppendPosition}");
        }
    }
}
