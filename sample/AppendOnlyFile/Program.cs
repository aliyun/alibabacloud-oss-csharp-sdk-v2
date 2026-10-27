using System.Text;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.AppendOnlyFile
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

            var cfg = OSS.Configuration.LoadDefault();
            cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
            cfg.Region = option.Region;
            if (option.Endpoint != null)
            {
                cfg.Endpoint = option.Endpoint;
            }

            using var client = new OSS.Client(cfg);

            try
            {
                // Open (or create) the object for appending. CreateParameter is applied only on the
                // first write that creates the object.
                using var file = await OSS.AppendOnlyFile.OpenAsync(client, option.Bucket!, option.Key!, o =>
                {
                    o.CreateParameter = new OSS.Models.AppendObjectRequest
                    {
                        ContentType = "text/plain"
                    };
                });

                var chunk1 = Encoding.UTF8.GetBytes("hi,");
                await file.WriteAsync(chunk1, 0, chunk1.Length);

                var chunk2 = Encoding.UTF8.GetBytes("oss!");
                await file.WriteAsync(chunk2, 0, chunk2.Length);

                // Append from an arbitrary stream too.
                using var src = new MemoryStream(Encoding.UTF8.GetBytes(" from stream"));
                var written = await file.WriteFromAsync(src);

                var info = file.Stat();
                Console.WriteLine("AppendOnlyFile done");
                Console.WriteLine($"Name: {info.Name}");
                Console.WriteLine($"Size: {info.Size}");
                Console.WriteLine($"Bytes appended from stream: {written}");
            }
            catch (OSS.ServiceException e)
            {
                Console.WriteLine($"ServiceException: code={e.ErrorCode}, status={e.StatusCode}, message={e.ErrorMessage}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
