# Alibaba Cloud OSS SDK for C# v2

[![GitHub version](https://badge.fury.io/gh/aliyun%2Falibabacloud-oss-csharp-sdk-v2.svg)](https://badge.fury.io/gh/aliyun%2Falibabacloud-oss-csharp-sdk-v2)

alibabacloud-oss-csharp-sdk-v2 是OSS在C#编译语言下的第二版SDK

## [English](README.md)

## 关于
> - 此C# SDK基于[阿里云对象存储服务](http://www.aliyun.com/product/oss/)官方API构建。
> - 阿里云对象存储（Object Storage Service，简称OSS），是阿里云对外提供的海量，安全，低成本，高可靠的云存储服务。
> - OSS适合存放任意文件类型，适合各种网站、开发企业及开发者使用。
> - 使用此SDK，用户可以方便地在任何应用、任何时间、任何地点上传，下载和管理数据。

## 运行环境
 - 适用于`.NET Framework 471`及以上版本
 - 适用于`.NET Standard 2.0`及以上版本
 - 适用于`.NET5.0`及以上版本

## 安装方法
### 通过 NuGet 安装
> - 如果您的Visual Studio没有安装NuGet，请先安装 [NuGet](http://docs.nuget.org/docs/start-here/installing-nuget).
> - 安装好NuGet后，先在`Visual Studio`中新建或者打开已有的项目，然后选择`<工具>`－`<NuGet程序包管理器>`－`<管理解决方案的NuGet程序包>`，
> - 搜索`AlibabaCloud.SDK.OSS.V2`，在结果中找到`AlibabaCloud.SDK.OSS.V2`，选择最新版本，点击安装，成功后添加到项目应用中。

### 通过 GitHub 安装
> - 如果没有安装git，请先安装 [git](https://git-scm.com/downloads)
> - git clone https://github.com/aliyun/alibabacloud-oss-csharp-sdk-v2.git
> - 下载好源码后，按照`项目引入方式安装`即可

### 项目引入方式安装
> - 如果是下载了SDK包或者从GitHub上下载了源码，希望源码安装，可以右键`<解决方案>`，在弹出的菜单中点击`<添加>`->`<现有项目>`。
> - 在弹出的对话框中选择`AlibabaCloud.OSS.V2.csproj`文件，点击打开。
> - 接下来右键`<您的项目>`－`<引用>`，选择`<添加引用>`，在弹出的对话框选择`<项目>`选项卡后选中`AlibabaCloud.OSS.V2`项目，点击确定即可。

## 快速使用
#### 获取存储空间列表（List Buckets）
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

#### 获取文件列表（List Objects）
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

#### 上传文件（Put Object）
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

## 更多示例
请参看`sample`目录

### 运行示例
> - 安装`dotnet`
> - 进入示例程序目录 `sample`。
> - ͨ通过环境变量，配置访问凭证, `export OSS_ACCESS_KEY_ID="your access key id"`, `export OSS_ACCESS_KEY_SECRET="your access key secrect"`
> - 以 ListBuckets 为例，执行 `dotnet run --project ListBuckets\ListBuckets.csproj -f net48 --region cn-shenzhen`。

## 许可协议
> - Apache-2.0, 请参阅 [许可文件](LICENSE)