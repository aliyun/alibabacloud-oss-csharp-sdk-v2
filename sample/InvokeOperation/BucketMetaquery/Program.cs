using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.InvokeOperation.BucketMetaquery
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

            [Option("operation", Required = true, HelpText = "The `name` of operation.")]
            public string? Operation { get; set; }
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
            var operation = option.Operation;

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

            switch (operation)
            {
                case "OpenMetaQuery":
                    await OpenMetaQueryAsync(client, bucket);
                    break;
                case "DoMetaQuery":
                    await DoMetaQueryAsync(client, bucket);
                    break;
                case "GetMetaQueryStatus":
                    await GetMetaQueryStatusAsync(client, bucket);
                    break;
                case "CloseMetaQuery":
                    await CloseMetaQueryAsync(client, bucket);
                    break;
                default:
                    throw new NotImplementedException($"{operation} is not supported");
            }
        }

        class EncodingStringWriter : StringWriter
        {
            // Need to subclass StringWriter in order to override Encoding
            public EncodingStringWriter(Encoding encoding) => Encoding = encoding;

            public override Encoding Encoding { get; }
        }

        // API Doc
        // english https://www.alibabacloud.com/help/en/oss/developer-reference/data-indexing
        // 简体中文 https://www.alibabacloud.com/help/zh/oss/developer-reference/data-indexing
        public static async Task OpenMetaQueryAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "OpenMetaQuery",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg=="},
                },
                Parameters = new Dictionary<string, string>{
                    {"comp", "add"},
                    {"metaQuery", ""},
                    // mode, Valid values: basic (default): Scalar search, semantic: Semantic search
                    {"mode", "basic"},
                    // The name of the RAM role used to access OSS
                    //{"role", "my-oss-role"},
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("OpenMetaQuery done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");
        }

        // Takes Scalar retrieval as an example
        public static async Task DoMetaQueryAsync(OSS.Client client, string? bucket)
        {
            // Scalar retrieval
            var metaQuery = new MetaQuery()
            {
                MaxResults = 100,
                Query = "{\"Field\": \"Size\",\"Value\": \"1048576\",\"Operation\": \"gt\"}",
                Sort = "Size",
                Order = "asc",
                Aggregations = new MetaQueryAggregations()
                {
                    Aggregations = [
                        new MetaQueryAggregation(){
                            Field = "Size",
                            Operation = "sum"
                        },
                        new MetaQueryAggregation(){
                            Field = "Size",
                            Operation = "max"
                        }
                    ]
                }
            };

            // to XML
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(metaQuery.GetType());
            var writer = new EncodingStringWriter(Encoding.UTF8);
            serializer.Serialize(writer, metaQuery, ns);
            writer.Flush();
            var xmlBytes = Encoding.UTF8.GetBytes(writer.ToString());

            var contentMD5 = "";
            using (var md5 = MD5.Create())
            {
                contentMD5 = Convert.ToBase64String(md5.ComputeHash(xmlBytes));
            }

            var input = new OSS.OperationInput
            {
                OperationName = "DoMetaQuery",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", contentMD5 },
                },
                Parameters = new Dictionary<string, string>{
                    {"comp", "query"},
                    {"metaQuery", ""},
                },
                Bucket = bucket,
                Body = new MemoryStream(xmlBytes)
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("DoMetaQuery done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");

            // from XML
            var seder = new XmlSerializer(typeof(MetaQueryResp));
            var xmlReader = XmlReader.Create(output.Body!);
            var metaQueryResp = seder.Deserialize(xmlReader) as MetaQueryResp;
            if (metaQueryResp != null)
            {
                foreach (var file in metaQueryResp.Files?.Files ?? [])
                {
                    Console.WriteLine($"File:{file.Filename}, {file.Size}, {file.OSSStorageClass}");
                }

                foreach (var aggregation in metaQueryResp.Aggregations?.Aggregations ?? [])
                {
                    Console.WriteLine($"Aggregation:{aggregation.Field}, {aggregation.Value}");
                }
            }
        }

        public static async Task GetMetaQueryStatusAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "GetMetaQueryStatus",
                Method = "GET",
                Parameters = new Dictionary<string, string>{
                    {"metaQuery", ""},
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            var serializer = new XmlSerializer(typeof(MetaQueryStatus));
            var metaQueryStatus = serializer.Deserialize(output.Body!) as MetaQueryStatus;

            Console.WriteLine("GetMetaQueryStatus done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");
            Console.WriteLine($"CreateTime: {metaQueryStatus?.CreateTime}");
            Console.WriteLine($"State: {metaQueryStatus?.State}");
        }

        public static async Task CloseMetaQueryAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "CloseMetaQuery",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg=="},
                },
                Parameters = new Dictionary<string, string>{
                    {"comp", "delete"},
                    {"metaQuery", ""},
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("CloseMetaQuery done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");
        }
    }
}
