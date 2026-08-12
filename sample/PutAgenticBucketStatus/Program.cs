using CommandLine;
using OSS = AlibabaCloud.OSS.V2;
using Agentic = AlibabaCloud.OSS.V2.Agentic;

namespace Sample.PutAgenticBucketStatus
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

            [Option("bucket", Required = true, HelpText = "The short `name` of the agentic bucket.")]
            public string? Bucket { get; set; }

            [Option("status", Required = true, HelpText = "The `status` of the agentic bucket, Enabled or Disabled.")]
            public string? Status { get; set; }
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
                var result = await client.PutAgenticBucketStatusAsync(new Agentic.Models.PutAgenticBucketStatusRequest
                {
                    Bucket = option.Bucket,
                    AgenticBucketStatus = new Agentic.Models.AgenticBucketStatus
                    {
                        Status = option.Status
                    }
                });

                Console.WriteLine("PutAgenticBucketStatus done");
                Console.WriteLine($"StatusCode: {result.StatusCode}");
                Console.WriteLine($"RequestId: {result.RequestId}");
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
