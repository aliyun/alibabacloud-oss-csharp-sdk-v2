using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.AliyunCredentialsUsage
{
    public class Program
    {

        public class Options
        {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }
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

            // Aliyun Credentials
            // Take RamRoleArn for example
            // For more examples, please refer to https://github.com/aliyun/credentials-csharp.
            var credConfig = new Aliyun.Credentials.Models.Config()
            {
                // Which type of credential you want
                Type = "ram_role_arn",
                // AccessKeyId of your account
                AccessKeyId = "<AccessKeyId>",
                // AccessKeySecret of your account
                AccessKeySecret = "<AccessKeySecret>",
                // Format: acs:ram::USER_Id:role/ROLE_NAME
                // RoleArn can be replaced by setting environment variable: ALIBABA_CLOUD_ROLE_ARN
                RoleArn = "<RoleArn>",
                // Role Session Name
                RoleSessionName = "<RoleSessionName>",
                // Optional, limit the permissions of STS Token
                Policy = "<Policy>",
                // Optional, limit the Valid time of STS Token
                RoleSessionExpiration = 3600,
            };
            var credClient = new Aliyun.Credentials.Client(credConfig);

            // Cast to OSS Credentials Provider
            var credentialsProvider = new OSS.Credentials.CredentialsProvideFunc(() =>
            {
                var credential = credClient.GetCredential();
                return new OSS.Credentials.Credentials(
                    credential.AccessKeyId,
                    credential.AccessKeySecret,
                    credential.SecurityToken);
            });

            // Using the SDK's default configuration
            // loading credentials values from the environment variables
            var cfg = OSS.Configuration.LoadDefault();
            cfg.Region = region;
            cfg.CredentialsProvider = credentialsProvider;

            if (endpoint != null)
            {
                cfg.Endpoint = endpoint;
            }

            using var client = new OSS.Client(cfg);

            // Create the Paginator for the ListBuckets operation.
            var paginator = client.ListBucketsPaginator(new OSS.Models.ListBucketsRequest());

            // Iterate through the bucket pages
            Console.WriteLine("Buckets:");
            await foreach (var page in paginator.IterPageAsync())
            {
                foreach (var bucket in page.Buckets ?? [])
                {
                    Console.WriteLine($"Bucket:{bucket.Name}, {bucket.StorageClass}, {bucket.Location}");
                }
            }
        }
    }
}
