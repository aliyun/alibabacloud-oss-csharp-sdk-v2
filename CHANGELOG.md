# ChangeLog - Alibaba Cloud OSS SDK for C# v2

## 版本号：0.2.0 日期：2025-06-26
### 变更内容
- Feature：Add SealAppendObject API support
- Feature：Add DisableAutoDetectMimeType configuration option
- Feature：Add clock skew correction support
- Feature：Add NativeAOT/Trimming support
- Fix：Fix metadata serialization NullReferenceException
- Fix：Fix callback support for PutObject and CompleteMultipartUpload
- Upadte：Switch UrlEncode/UrlDecode to Uri.EscapeDataString/UnescapeDataString

## 版本号：0.1.2 日期：2025-06-12
### 变更内容
- Feature：Add BoundedStream
- Break Change：Change CredentialsProvideFunc to CredentialsProviderFunc
- Break Change：Change StaticCredentialsProvide to StaticCredentialsProvider

## 版本号：0.1.1 日期：2025-04-25
### 变更内容
- Fix：Encode query parameters that contain special characters correctly

## 版本号：0.1.0 日期：2025-03-05
### 变更内容
- Feature：Add credentials provider
- Feature：Add retryer
- Feature：Add signer v4/v1
- Feature：Add annotation for 8.x
- Feature：Add bucket's basic api
- Feature：Add object's api
- Feature：Add presigner
- Feature：Add paginator
- Feature：Add IsObjectExistAsync/IsBucketExistAsync api
- Feature：Add PutObjectFromFileAsync/GetObjectToFileAsync api
