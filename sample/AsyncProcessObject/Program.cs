using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.AsyncProcessObject
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

            // Specify the name of the bucket to store the converted file
            var targetBucket = bucket!;
            // Specify the name of the converted file
            var targetKey = $"process-{key}";
            // Build document processing style strings and document transformation processing parameters
            // Define the processing rules for converting the source Docx document to a PNG image
            var style = "doc/convert,target_png,source_docx";

            var targetNameBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(targetBucket));
            var targetKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(targetKey));

            var process = $"{style}|sys/saveas,b_{targetNameBase64},o_{targetKeyBase64}/notify";

            var result = await client.AsyncProcessObjectAsync(new OSS.Models.AsyncProcessObjectRequest()
            {
                Bucket = bucket,
                Key = key,
                Process = process
            });

            Console.WriteLine("AsyncProcessObject done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine($"RequestId: {result.RequestId}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
            Console.WriteLine($"ProcessResult: {result.ProcessResult}");
        }
    }
}
