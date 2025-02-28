using OSS = AlibabaCloud.OSS.V2;
using CommandLine;

namespace Sample.GetObject {
    public class Program {

        public class Options {
            [Option("region", Required = true, HelpText = "The region in which the bucket is located.")]
            public string? Region { get; set; }

            [Option("endpoint", Required = false, HelpText = "The domain names that other services can use to access OSS.")]
            public string? Endpoint { get; set; }

            [Option("bucket", Required = true, HelpText = "The `name` of the bucket.")]
            public string? Bucket { get; set; }

            [Option("key", Required = true, HelpText = "The `name` of the object.")]
            public string? Key { get; set; }
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

            // default is streaming mode
            var result = await client.GetObjectAsync(new OSS.Models.GetObjectRequest() {
                Bucket = option.Bucket,
                Key = option.Key,
            });

            // real all data into memory
            //var result = await client.GetObjectAsync(new OSS.Models.GetObjectRequest() {
            //    Bucket = option.Bucket,
            //    Key = option.Key,
            //},System.Net.Http.HttpCompletionOption.ResponseContentRead);

            using var body = result.Body;
            var reader = new StreamReader(body!);
            var data = reader.ReadToEnd();

            Console.WriteLine("GetObject done");
            Console.WriteLine($"StatusCode: {result.StatusCode}");
            Console.WriteLine($"RequestId: {result.RequestId}");
            Console.WriteLine("Response Headers:");
            result.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }
    }
}