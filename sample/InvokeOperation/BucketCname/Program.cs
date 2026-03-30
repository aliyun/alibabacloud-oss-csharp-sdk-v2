using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CommandLine;
using OSS = AlibabaCloud.OSS.V2;

namespace Sample.InvokeOperation.BucketCname
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
                case "PutCnameAsync":
                    await PutCnameAsync(client, bucket);
                    break;
                case "ListCnameAsync":
                    await ListCnameAsync(client, bucket);
                    break;
                case "DeleteCnameAsync":
                    await DeleteCnameAsync(client, bucket);
                    break;
                case "GetCnameTokenAsync":
                    await GetCnameTokenAsync(client, bucket);
                    break;
                case "CreateCnameTokenAsync":
                    await CreateCnameTokenAsync(client, bucket);
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
        // english https://www.alibabacloud.com/help/en/oss/developer-reference/bucket-cname
        // 简体中文 https://www.alibabacloud.com/help/zh/oss/developer-reference/bucket-cname
        public static async Task PutCnameAsync(OSS.Client client, string? bucket)
        {

            var cnameConfiguration = new BucketCnameConfiguration()
            {
                Cname = new Cname()
                {
                    Domain = "your-name.com"
                }
            };

            // to XML
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(cnameConfiguration.GetType());
            var writer = new EncodingStringWriter(Encoding.UTF8);
            serializer.Serialize(writer, cnameConfiguration, ns);
            writer.Flush();
            var xmlBytes = Encoding.UTF8.GetBytes(writer.ToString());

            var contentMD5 = "";
            using (var md5 = MD5.Create())
            {
                contentMD5 = Convert.ToBase64String(md5.ComputeHash(xmlBytes));
            }

            var input = new OSS.OperationInput
            {
                OperationName = "PutCname",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", contentMD5},
                },
                Parameters = new Dictionary<string, string>{
                    {"cname", ""},
                    {"comp", "add"},
                },
                Bucket = bucket,
                Body = new MemoryStream(xmlBytes)
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("PutCname done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");
        }

        public static async Task ListCnameAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "ListCname",
                Method = "GET",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg=="},
                },
                Parameters = new Dictionary<string, string>{
                    {"cname", ""},
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("ListCname done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");

            // from XML
            var seder = new XmlSerializer(typeof(ListCnameResult));
            var xmlReader = XmlReader.Create(output.Body!);
            var listCnameResult = seder.Deserialize(xmlReader) as ListCnameResult;
            if (listCnameResult != null)
            {
                //Print
            }
        }

        public static async Task DeleteCnameAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "DeleteCname",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg=="},
                },
                Parameters = new Dictionary<string, string>{
                    {"cname", ""},
                    {"comp", "delete"},
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("DeleteCnameAsync done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");
        }


        public static async Task GetCnameTokenAsync(OSS.Client client, string? bucket)
        {
            var input = new OSS.OperationInput
            {
                OperationName = "GetCnameToken",
                Method = "GET",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg=="},
                },
                Parameters = new Dictionary<string, string>{
                    {"comp", "token"},
                    // TODO set cname name
                    {"cname", "your-cname.com" }
                },
                Bucket = bucket,
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("GetCnameTokenAsync done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");

            // from XML
            var seder = new XmlSerializer(typeof(CnameToken));
            var xmlReader = XmlReader.Create(output.Body!);
            var cnameToken = seder.Deserialize(xmlReader) as CnameToken;
            if (cnameToken != null)
            {
                //Print
            }
        }

        public static async Task CreateCnameTokenAsync(OSS.Client client, string? bucket)
        {
            var cnameConfiguration = new BucketCnameConfiguration()
            {
                Cname = new Cname()
                {
                    Domain = "your-name.com"
                }
            };

            // to XML
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(cnameConfiguration.GetType());
            var writer = new EncodingStringWriter(Encoding.UTF8);
            serializer.Serialize(writer, cnameConfiguration, ns);
            writer.Flush();
            var xmlBytes = Encoding.UTF8.GetBytes(writer.ToString());

            var contentMD5 = "";
            using (var md5 = MD5.Create())
            {
                contentMD5 = Convert.ToBase64String(md5.ComputeHash(xmlBytes));
            }

            var input = new OSS.OperationInput
            {
                OperationName = "CreateCnameToken",
                Method = "POST",
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Content-Type", "application/xml" },
                    { "Content-MD5", contentMD5},
                },
                Parameters = new Dictionary<string, string>{
                    {"cname", ""},
                    {"comp", "token"},
                },
                Bucket = bucket,
                Body = new MemoryStream(xmlBytes)
            };

            var output = await client.InvokeOperationAsync(input);

            Console.WriteLine("CreateCnameTokenAsync done");
            Console.WriteLine($"StatusCode: {output.StatusCode}");
            Console.WriteLine($"RequestId: {output.Headers?["x-oss-request-id"]}");

            // from XML
            var seder = new XmlSerializer(typeof(CnameToken));
            var xmlReader = XmlReader.Create(output.Body!);
            var cnameToken = seder.Deserialize(xmlReader) as CnameToken;
            if (cnameToken != null)
            {
                //Print
            }
        }
    }
}
