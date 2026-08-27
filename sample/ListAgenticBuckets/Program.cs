using CommandLine;
using OSS = AlibabaCloud.OSS.V2;
using Agentic = AlibabaCloud.OSS.V2.Agentic;

namespace Sample.ListAgenticBuckets
{
    public class Program
    {
        public class Options
        {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }

            [Option("account-id", Required = true, HelpText = "The ID of the Alibaba Cloud account.")]
            public string? AccountId { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Errors.Any())
            {
                Environment.Exit(1);
            }
            var option = parserResult.Value;

            // Using the SDK's default configuration
            // loading credentials values from the environment variables
            var cfg = OSS.Configuration.LoadDefault();
            cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
            cfg.Region = option.Region;
            cfg.AccountId = option.AccountId;

            if (option.Endpoint != null)
            {
                cfg.Endpoint = option.Endpoint;
            }

            using var client = new Agentic.AgenticBucketClient(cfg);

            try
            {
                // Create the Paginator for the ListAgenticBuckets operation.
                var paginator = client.ListAgenticBucketsPaginator(new Agentic.Models.ListAgenticBucketsRequest());

                Console.WriteLine("AgenticBuckets:");
                await foreach (var page in paginator.IterPageAsync())
                {
                    foreach (var bucket in page.AgenticBuckets ?? [])
                    {
                        Console.WriteLine($"Bucket: {bucket.Name}, {bucket.StorageClass}, {bucket.CreateTime}");
                    }
                }
            }
            catch (OSS.ServiceException se)
            {
                Console.WriteLine($"ServiceException: StatusCode={se.StatusCode}, Code={se.ErrorCode}, Message={se.ErrorMessage}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
