using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.ReadOnlyFile
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
                // Open the object as a read-only, seekable stream with background prefetch enabled.
                using var file = await OSS.ReadOnlyFile.OpenAsync(client, option.Bucket!, option.Key!, o =>
                {
                    o.EnablePrefetch = true;
                });

                var info = file.Stat();
                Console.WriteLine($"Name: {info.Name}");
                Console.WriteLine($"Size: {info.Size}");
                Console.WriteLine($"LastModified: {info.LastModified}");

                // Read the whole object.
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                Console.WriteLine($"Read {ms.Length} bytes sequentially.");

                // Seek back to the middle and read a small chunk.
                if (file.Length >= 16)
                {
                    file.Seek(file.Length / 2, SeekOrigin.Begin);
                    var buf = new byte[16];
                    var n = await file.ReadAsync(buf, 0, buf.Length);
                    Console.WriteLine($"Read {n} bytes after seeking to the middle.");
                }

                Console.WriteLine("ReadOnlyFile done");
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
