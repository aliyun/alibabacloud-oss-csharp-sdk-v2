using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.Uploader
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

            [Option("filepath", Required = true, HelpText = "The path of a file to upload.")]
            public string? FilePath { get; set; }

            [Option("partsize", HelpText = "The part size in bytes.")]
            public long? PartSize { get; set; }

            [Option("parallel", HelpText = "The number of parts uploaded concurrently.")]
            public int? ParallelNum { get; set; }
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

            if (option.Endpoint != null)
            {
                cfg.Endpoint = option.Endpoint;
            }

            using var client = new OSS.Client(cfg);

            // Create an Uploader that transfers the file as concurrent multipart parts.
            var uploader = new OSS.Uploader(client, o =>
            {
                if (option.PartSize.HasValue) o.PartSize = option.PartSize.Value;
                if (option.ParallelNum.HasValue) o.ParallelNum = option.ParallelNum.Value;
            });

            var request = new OSS.Models.PutObjectRequest
            {
                Bucket = option.Bucket,
                Key = option.Key,
                // Report upload progress.
                ProgressFn = (increment, transferred, total) =>
                {
                    var pct = total > 0 ? transferred * 100 / total : 0;
                    Console.WriteLine($"upload progress: {pct}% ({transferred}/{total})");
                }
            };

            try
            {
                var result = await uploader.UploadFileAsync(request, option.FilePath!);

                Console.WriteLine("Uploader done");
                Console.WriteLine($"ETag: {result.ETag}");
                Console.WriteLine($"UploadId: {result.UploadId}");
                Console.WriteLine($"HashCrc64: {result.HashCrc64}");
                Console.WriteLine($"StatusCode: {result.StatusCode}");
                Console.WriteLine($"RequestId: {result.RequestId}");
            }
            catch (OSS.OperationException oe)
            {
                Console.WriteLine($"Operation failed: {oe.Message}");
                if (oe.InnerException is OSS.ServiceException se)
                {
                    Console.WriteLine($"ErrorCode: {se.ErrorCode}");
                    Console.WriteLine($"ErrorMessage: {se.ErrorMessage}");
                    Console.WriteLine($"RequestId: {se.RequestId}");
                }
                Environment.Exit(1);
            }
        }
    }
}
