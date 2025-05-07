using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.PutObjectAcl
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

            [Option("acl", Required = true, HelpText = "The ACL that you want to configure or modify for the object.")]
            public string? Acl { get; set; }
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
            var acl = option.Acl;

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

            var result = await client.PutObjectAclAsync(new OSS.Models.PutObjectAclRequest()
            {
                Bucket = bucket,
                Key = key,
                Acl = acl
            });

            Console.WriteLine("PutObjectAcl done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine($"RequestId: {result.RequestId}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }
    }
}
