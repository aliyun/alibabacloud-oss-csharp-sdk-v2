using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.CopyObject
{
    public class Program
    {

        public class Options
        {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }

            [Option("src-bucket", Required = true, HelpText = "The `name` of the source bucket.")]
            public string? SrcBucket { get; set; }

            [Option("src-key", Required = true, HelpText = "The `name` of the source object.")]
            public string? SrcKey { get; set; }

            [Option("dst-bucket", Required = true, HelpText = "The `name` of the destination bucket.")]
            public string? DstBucket { get; set; }

            [Option("dst-key", Required = true, HelpText = "The `name` of the destination object.")]
            public string? DstKey { get; set; }

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
            var endpoint = option.Endpoint;
            var srcBucket = option.SrcBucket;
            var srcKey = option.SrcKey;
            var dstBucket = option.DstBucket;
            var dstKey = option.DstKey;

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

            var result = await client.CopyObjectAsync(new OSS.Models.CopyObjectRequest()
            {
                Bucket = dstBucket,
                Key = dstKey,
                SourceBucket = srcBucket,
                SourceKey = srcKey
            });

            Console.WriteLine("CopyObject done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine($"RequestId: {result.RequestId}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }
    }
}
