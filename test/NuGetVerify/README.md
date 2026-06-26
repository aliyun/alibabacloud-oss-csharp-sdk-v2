# NuGet Package Verification

Verifies the published `AlibabaCloud.OSS.V2` NuGet package works correctly across target frameworks.

## Coverage

- Service: DescribeRegions, ListBuckets
- Bucket: Put, GetInfo, GetStat, GetLocation, Delete, GetAcl
- Object: Put, Head, GetMeta, Get, Copy, Delete
- Object Acl: Put, Get
- Object Tagging: Put, Get, Delete
- Object Symlink: Put, Get
- AppendObject / SealAppendObject
- Multipart: Initiate, UploadPart, UploadPartCopy, ListParts, ListMultipartUploads, Complete, Abort
- Presigner: pre-signed URL generation and access
- Paginator: ListObjects, ListObjectsV2, ListBuckets
- Extensions: IsBucketExist, IsObjectExist
- DeleteMultipleObjects

## Environment Variables

| Variable | Description |
|----------|-------------|
| `OSS_ACCESS_KEY_ID` | AccessKey ID (required) |
| `OSS_ACCESS_KEY_SECRET` | AccessKey Secret (required) |
| `OSS_SESSION_TOKEN` | STS Token (optional) |

## Usage

```bash
# .NET 8.0 (JIT)
dotnet run --framework net8.0

# .NET Framework 4.8 (JIT)
dotnet run --framework net48

# .NET 9.0 (AOT)
dotnet publish -c Release --framework net9.0
bin/Release/net9.0/win-x64/publish/NuGetVerify.exe
```

> AOT compilation requires `vswhere.exe` in PATH, or run from a VS Developer Command Prompt.
