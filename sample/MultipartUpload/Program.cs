using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.MultipartUpload
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

            [Option("partsize", HelpText = "The part size, 512*1024 as default.")]
            public long? PartSize { get; set; }

            [Option("filepath", Required = true, HelpText = "The path of a file to upload.")]
            public string? FilePath { get; set; }
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
            var filePath = option.FilePath!;
            long partSize = option.PartSize ?? 512*1024;

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

            var initResult = await client.InitiateMultipartUploadAsync(new()
            {
                Bucket = bucket,
                Key = key
            });

            // upload
            using var file = File.OpenRead(filePath);
            long fileSize = file.Length;
            long partNumber = 1;

            var uploadParts = new List<OSS.Models.UploadPart>();

            for (long offset = 0; offset < fileSize; offset += partSize)
            {
                var size = Math.Min(partSize, fileSize - offset);
                var upResult = await client.UploadPartAsync(new()
                {
                    Bucket = bucket,
                    Key = key,
                    PartNumber = partNumber,
                    UploadId = initResult.UploadId,
                    Body = new OSS.IO.BoundedStream(file, offset, size)
                });
                uploadParts.Add(new () { PartNumber = partNumber, ETag = upResult.ETag });
                partNumber++;
            }

            // complete
            uploadParts.Sort((left, right) => { return (left.PartNumber > right.PartNumber) ? 1 : -1; });
            var cmResult = await client.CompleteMultipartUploadAsync(new()
            {
                Bucket = bucket,
                Key = key,
                UploadId = initResult.UploadId,
                CompleteMultipartUpload = new ()
                {
                    Parts = uploadParts
                }
            });

            Console.WriteLine("MultipartUpload done");
            Console.WriteLine($"StatusCode: {cmResult.StatusCode}");
            Console.WriteLine($"RequestId: {cmResult.RequestId}");
            Console.WriteLine("Response Headers:");
            cmResult.Headers.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }
    }
}
