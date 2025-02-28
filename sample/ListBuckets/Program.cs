using OSS = AlibabaCloud.OSS.V2;
using CommandLine;

namespace Sample.ListBuckets {
    public class Program {

        public class Options {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }
        }

        public static async Task Main(string[] args) {

            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Errors.Any()) {
                Environment.Exit(1);
            }
            var option = parserResult.Value;

            // Using the SDK's default configuration
            // loading credentials values from the environment variables
            var cfg = OSS.Configuration.LoadDefault();
            cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
            cfg.Region = option.Region;
            cfg.Endpoint = option.Endpoint;

            using var client = new OSS.Client(cfg);

            // Create the Paginator for the ListBuckets operation.
            var paginator = client.ListBucketsPaginator(new OSS.Models.ListBucketsRequest());

            // Iterate through the bucket pages
            Console.WriteLine("Buckets:");
            await foreach (var page in paginator.IterPageAsync()) {
                foreach (var bucket in page.Buckets ?? []) {
                    Console.WriteLine($"Bucket:{bucket.Name}, {bucket.StorageClass}, {bucket.Location}");
                }
            }
        }
    }
}