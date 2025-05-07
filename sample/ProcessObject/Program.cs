using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.ProcessObject
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

            // Specify the name of the bucket to store the processed images
            // the bucket must be in the same region as the bucket where the original image resides
            var targetBucket = bucket!;
            // Specify the name of the processed image.
            var targetKey = $"process-{key}";
            // Scale the image to a fixed width and height of 100 px
            var style = "image/resize,m_fixed,w_100,h_100";

            var targetNameBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(targetBucket));
            var targetKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(targetKey));

            var process = $"{style}|sys/saveas,o_{targetKeyBase64},b_{targetNameBase64}";

            var result = await client.ProcessObjectAsync(new OSS.Models.ProcessObjectRequest()
            {
                Bucket = bucket,
                Key = key,
                Process = process
            });

            Console.WriteLine("ProcessObject done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine($"RequestId: {result.RequestId}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }
    }
}
