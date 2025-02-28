# Alibaba Cloud OSS SDK for C# v2

[![GitHub version](https://badge.fury.io/gh/aliyun%2Falibabacloud-oss-csharp-sdk-v2.svg)](https://badge.fury.io/gh/aliyun%2Falibabacloud-oss-csharp-sdk-v2)

alibabacloud-oss-csharp-sdk-v2 is the Developer Preview for the v2 of the OSS SDK for the C# programming language

## [简体中文](README-CN.md)

## About
> - This C# SDK is based on the official APIs of [Alibaba Cloud OSS](http://www.aliyun.com/product/oss/).
> - Alibaba Cloud Object Storage Service (OSS) is a cloud storage service provided by Alibaba Cloud, featuring massive capacity, security, a low cost, and high reliability.
> - The OSS can store any type of files and therefore applies to various websites, development enterprises and developers.
> - With this SDK, you can upload, download and manage data on any app anytime and anywhere conveniently.

## Running Environment
 - Applicable to`.NET Framework 471`or above
 - Applicable to`.NET Standard 2.0`or above
 - Applicable to`.NET5.0`or above

## Installing
### Install the sdk through NuGet
> - If NuGet hasn't been installed for Visual Studio, install [NuGet](http://docs.nuget.org/docs/start-here/installing-nuget) first.
> - After NuGet is installed, access Visual Studio to create a project or open an existing project, and then select `TOOLS` > `NuGet Package Manager` > `Manage NuGet Packages for Solution`.
> - Type `AlibabaCloud.SDK.OSS.V2`，in the search box and click *Search*, find `AlibabaCloud.SDK.OSS.V2` in the search results, select the latest version, and click *Install*. After installation, the SDK is added to the project.

### Install the SDK through GitHub
> - If Git hasn't been installed, install [Git](https://git-scm.com/downloads) first.
> - Clone project via `git clone https://github.com/aliyun/alibabacloud-oss-csharp-sdk-v2.git`.
> - After the source code is downloaded, install the SDK by entering `Install via Project Introduction`.

### Install the SDK through project introduction
> - If you have downloaded the SDK package or the source code from GitHub and you want to install the SDK package using the source code, you can right click `Solution Explorer` and select `Add` > `Existing Projects` from the pop-up menu.
> - In the pop-up dialog box, select the `AlibabaCloud.OSS.V2.csproj` file, and click *Open*.
> - Right click *Your Projects* and select `Add Reference`. In the `Reference Manager` dialog box, click the `Projects` tab, select the `AlibabaCloud.OSS.V2` project, and click *OK*.

## Getting Started
#### List Buckets
```csharp
using OSS = AlibabaCloud.OSS.V2;

var region = "cn-hangzhou";

// Using the SDK's default configuration
// loading credentials values from the environment variables
var cfg = OSS.Configuration.LoadDefault();
cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
cfg.Region = region;

using var client = new OSS.Client(cfg);

// Create the Paginator for the ListBuckets operation.
var paginator = client.ListBucketsPaginator(new OSS.Models.ListBucketsRequest());

// Iterate through the bucket pages
Console.WriteLine("Buckets:");
await foreach (var page in paginator.IterPageAsync())
{
    foreach (var bucket in page.Buckets ?? [])
    {
        Console.WriteLine($"Bucket:{bucket.Name}, {bucket.StorageClass}, {bucket.Location}");
    }
}
```

#### List Objects
```csharp
using OSS = AlibabaCloud.OSS.V2;

var region = "cn-hangzhou";
var bucket = "your bucket name";

// Using the SDK's default configuration
// loading credentials values from the environment variables
var cfg = OSS.Configuration.LoadDefault();
cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
cfg.Region = region;

using var client = new OSS.Client(cfg);

// Create the Paginator for the ListObjects operation.
var paginator = client.ListObjectsV2Paginator(new OSS.Models.ListObjectsV2Request()
{
    Bucket = bucket
});

// Lists all objects in a bucket
Console.WriteLine("Objects:");
await foreach (var page in paginator.IterPageAsync())
{
    foreach (var content in page.Contents ?? [])
    {
        Console.WriteLine($"Object:{content.Key}, {content.Size}, {content.LastModified}");
    }
}
```

#### Put Object
```csharp
using System.Text;
using OSS = AlibabaCloud.OSS.V2;

var region = "cn-hangzhou";
var bucket = "your bucket name";
var key = "your object name";

// Using the SDK's default configuration
// loading credentials values from the environment variables
var cfg = OSS.Configuration.LoadDefault();
cfg.CredentialsProvider = new OSS.Credentials.EnvironmentVariableCredentialsProvider();
cfg.Region = region;

using var client = new OSS.Client(cfg);

var content = "hi oss";

var result = await client.PutObjectAsync(new()
{
    Bucket = bucket,
    Key = key,
    Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
});

Console.WriteLine($"PutObject done, StatusCode:{result.StatusCode}, RequestId:{result.RequestId}.");
```

##  Complete Example
More example projects can be found in the `sample` folder

### Running Example
> - install `dotnet`
> - Go to the sample code folder `sample`。
> - Configure credentials values from the environment variables, like `export OSS_ACCESS_KEY_ID="your access key id"`, `export OSS_ACCESS_KEY_SECRET="your access key secrect"`
> - Take ListBuckets as an example，run`dotnet run --project ListBuckets\ListBuckets.csproj -f net48 --region cn-shenzhen` command.

## License
> - Apache-2.0, see [license file](LICENSE)