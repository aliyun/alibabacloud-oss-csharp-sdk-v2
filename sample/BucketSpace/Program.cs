using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;
using Agentic = AlibabaCloud.OSS.V2.Agentic;

namespace Sample.BucketSpace
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

            [Option("bucket", Required = true, HelpText = "The short `name` of the bucket space.")]
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

            // Mode 1: a dedicated client that resolves the short bucket name to
            // "{bucket}-{accountId}-{region}-bs-apsr" on every request.
            using var spaceClient = Agentic.AgenticBucketClient.NewBucketSpaceClient(cfg);

            var putResult = await spaceClient.PutObjectAsync(new OSS.Models.PutObjectRequest
            {
                Bucket = option.Bucket,
                Key = option.Key,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello agentic"))
            });

            Console.WriteLine("[mode1] PutObject done");
            Console.WriteLine($"[mode1] StatusCode: {putResult.StatusCode}, RequestId: {putResult.RequestId}");

            // Mode 2: a plain client plus BucketSpaceHelper, which resolves the full
            // bucket name so you keep control over the endpoint and the client itself.
            var helper = new Agentic.BucketSpaceHelper(cfg);
            var fullBucketName = helper.ToBucketName(option.Bucket!);
            Console.WriteLine($"[mode2] resolved bucket name: {fullBucketName}");

            using var client = new OSS.Client(cfg);

            var getResult = await client.GetObjectAsync(new OSS.Models.GetObjectRequest
            {
                Bucket = fullBucketName,
                Key = option.Key
            });

            using var body = getResult.Body;
            var content = new StreamReader(body!).ReadToEnd();

            Console.WriteLine("[mode2] GetObject done");
            Console.WriteLine($"[mode2] StatusCode: {getResult.StatusCode}, RequestId: {getResult.RequestId}");
            Console.WriteLine($"[mode2] content: {content}");
        }
    }
}
