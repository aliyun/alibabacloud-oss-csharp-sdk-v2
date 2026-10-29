using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Shared helpers for the resumable-transfer checkpoint files.
    /// The on-disk format is a self-contained, deterministic line-based text format
    /// (Magic line, MD5 line, then ordered "Key=EscapedValue" data lines). It is only
    /// read and written by this SDK, so no cross-language compatibility is required.
    /// A fixed field order keeps the MD5 stable, and avoiding reflection-based JSON keeps
    /// the code trim/AOT safe on net8+.
    /// </summary>
    internal static class CheckpointHelper
    {
        internal static string Md5Hex(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        internal static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            var sb = new StringBuilder(value!.Length);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        internal static string Unescape(string value)
        {
            if (value.IndexOf('\\') < 0)
                return value;
            var sb = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '\\' && i + 1 < value.Length)
                {
                    var n = value[++i];
                    switch (n)
                    {
                        case '\\': sb.Append('\\'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        default: sb.Append(n); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Resolves the directory that holds the checkpoint file.
        /// Empty -> the system temp dir; an existing directory -> itself; otherwise its parent.
        /// </summary>
        internal static string ResolveDir(string? baseDir)
        {
            if (string.IsNullOrEmpty(baseDir))
                return Path.GetTempPath();
            if (Directory.Exists(baseDir))
                return baseDir!;
            return Path.GetDirectoryName(baseDir) ?? ".";
        }

        internal static string AbsPath(string filePath)
        {
            try { return Path.GetFullPath(filePath); }
            catch { return filePath; }
        }

        /// <summary>
        /// Parses the persisted checkpoint text. Returns (magic, storedMd5, computedMd5, fields)
        /// where fields preserves the data lines by key. Returns null on malformed input.
        /// </summary>
        internal static (string Magic, string StoredMd5, string ComputedMd5, Dictionary<string, string> Fields)? Parse(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length < 2)
                return null;

            string ReadValue(string line, string key)
            {
                var idx = line.IndexOf('=');
                if (idx < 0) return "";
                return line.Substring(idx + 1);
            }

            var magicLine = lines[0].TrimEnd('\r');
            var md5Line = lines[1].TrimEnd('\r');
            if (!magicLine.StartsWith("Magic=", StringComparison.Ordinal) ||
                !md5Line.StartsWith("MD5=", StringComparison.Ordinal))
                return null;

            var magic = ReadValue(magicLine, "Magic");
            var storedMd5 = ReadValue(md5Line, "MD5");

            var dataBuilder = new StringBuilder();
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 2; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length == 0)
                    continue;
                var raw = line.TrimEnd('\r');
                if (raw.Length == 0)
                    continue;
                dataBuilder.Append(raw).Append('\n');
                var idx = raw.IndexOf('=');
                if (idx < 0)
                    continue;
                var key = raw.Substring(0, idx);
                var val = raw.Substring(idx + 1);
                fields[key] = Unescape(val);
            }

            var computedMd5 = Md5Hex(dataBuilder.ToString());
            return (magic, storedMd5, computedMd5, fields);
        }
    }

    /// <summary>
    /// Checkpoint for resumable upload. Tracks the multipart uploadId so an interrupted
    /// upload can resume without re-uploading completed parts. Ported from Go V2
    /// (oss/checkpoint.go, uploadCheckpoint).
    /// </summary>
    internal sealed class UploadCheckpoint
    {
        public string CpDirPath = "";
        public string CpFilePath = "";
        public bool Loaded;

        // data
        public string FilePath = "";
        public long FileSize;
        public string FileLastModified = "";
        public string ObjectName = "";
        public long PartSize;
        public string UploadId = "";

        public static UploadCheckpoint Create(
            string bucket, string key, string filePath, string? baseDir,
            long fileSize, string fileLastModified, long partSize)
        {
            var name = bucket + "/" + key;
            var destHash = CheckpointHelper.Md5Hex("oss://" + name);
            var srcHash = CheckpointHelper.Md5Hex(CheckpointHelper.AbsPath(filePath));
            var dir = CheckpointHelper.ResolveDir(baseDir);

            return new UploadCheckpoint
            {
                CpDirPath = dir,
                CpFilePath = Path.Combine(dir, $"{srcHash}-{destHash}{Defaults.CheckpointFileSuffixUploader}"),
                FilePath = filePath,
                FileSize = fileSize,
                FileLastModified = fileLastModified,
                ObjectName = "oss://" + name,
                PartSize = partSize
            };
        }

        private string BuildDataText()
        {
            var sb = new StringBuilder();
            sb.Append("FilePath=").Append(CheckpointHelper.Escape(FilePath)).Append('\n');
            sb.Append("FileSize=").Append(FileSize.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("FileLastModified=").Append(CheckpointHelper.Escape(FileLastModified)).Append('\n');
            sb.Append("ObjectName=").Append(CheckpointHelper.Escape(ObjectName)).Append('\n');
            sb.Append("PartSize=").Append(PartSize.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("UploadId=").Append(CheckpointHelper.Escape(UploadId)).Append('\n');
            return sb.ToString();
        }

        public void Load()
        {
            if (!Directory.Exists(CpDirPath))
                throw new IOException($"Invalid checkpoint dir, {CpDirPath}");

            if (!File.Exists(CpFilePath))
                return;

            if (!Valid())
            {
                Remove();
                return;
            }

            Loaded = true;
        }

        private bool Valid()
        {
            try
            {
                var text = File.ReadAllText(CpFilePath, Encoding.UTF8);
                var parsed = CheckpointHelper.Parse(text);
                if (parsed == null)
                    return false;

                var (magic, storedMd5, computedMd5, f) = parsed.Value;
                if (!string.Equals(Defaults.CheckpointMagic, magic, StringComparison.Ordinal))
                    return false;
                if (!string.Equals(storedMd5, computedMd5, StringComparison.Ordinal))
                    return false;

                if (!string.Equals(Get(f, "ObjectName"), ObjectName, StringComparison.Ordinal)) return false;
                if (!string.Equals(Get(f, "FilePath"), FilePath, StringComparison.Ordinal)) return false;
                if (GetLong(f, "FileSize") != FileSize) return false;
                if (!string.Equals(Get(f, "FileLastModified"), FileLastModified, StringComparison.Ordinal)) return false;
                if (GetLong(f, "PartSize") != PartSize) return false;

                var uploadId = Get(f, "UploadId");
                if (string.IsNullOrEmpty(uploadId))
                    return false;

                UploadId = uploadId;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dump()
        {
            var dataText = BuildDataText();
            var md5 = CheckpointHelper.Md5Hex(dataText);
            var sb = new StringBuilder();
            sb.Append("Magic=").Append(Defaults.CheckpointMagic).Append('\n');
            sb.Append("MD5=").Append(md5).Append('\n');
            sb.Append(dataText);
            File.WriteAllText(CpFilePath, sb.ToString(), new UTF8Encoding(false));
        }

        public void Remove()
        {
            try { if (File.Exists(CpFilePath)) File.Delete(CpFilePath); }
            catch { /* ignore */ }
        }

        private static string Get(Dictionary<string, string> f, string key) => f.TryGetValue(key, out var v) ? v : "";
        private static long GetLong(Dictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) && long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : 0;
    }

    /// <summary>
    /// Checkpoint for resumable download. Tracks the contiguous downloaded offset and its
    /// CRC64 so an interrupted download can resume. Ported from Go V2
    /// (oss/checkpoint.go, downloadCheckpoint).
    /// </summary>
    internal sealed class DownloadCheckpoint
    {
        public string CpDirPath = "";
        public string CpFilePath = "";
        public bool VerifyData;
        public bool Loaded;

        // data - source
        public string ObjectName = "";
        public string VersionId = "";
        public string Range = "";
        public long ObjectSize;
        public string ObjectLastModified = "";
        public string ObjectETag = "";

        // data - destination
        public string FilePath = "";
        public long PartSize;

        // download info (mutable)
        public long Offset;
        public ulong Crc64;

        public static DownloadCheckpoint Create(
            string bucket, string key, string? versionId, string? range,
            string filePath, string? baseDir, bool verifyData,
            long objectSize, string lastModified, string etag, long partSize)
        {
            var name = bucket + "/" + key;
            var srcHash = CheckpointHelper.Md5Hex("oss://" + name + "\n" + (versionId ?? "") + "\n" + (range ?? ""));
            var destHash = CheckpointHelper.Md5Hex(CheckpointHelper.AbsPath(filePath));
            var dir = CheckpointHelper.ResolveDir(baseDir);

            return new DownloadCheckpoint
            {
                CpDirPath = dir,
                CpFilePath = Path.Combine(dir, $"{srcHash}-{destHash}{Defaults.CheckpointFileSuffixDownloader}"),
                VerifyData = verifyData,
                ObjectName = "oss://" + name,
                VersionId = versionId ?? "",
                Range = range ?? "",
                ObjectSize = objectSize,
                ObjectLastModified = lastModified,
                ObjectETag = etag,
                FilePath = filePath,
                PartSize = partSize
            };
        }

        private string BuildDataText()
        {
            var sb = new StringBuilder();
            sb.Append("ObjectName=").Append(CheckpointHelper.Escape(ObjectName)).Append('\n');
            sb.Append("VersionId=").Append(CheckpointHelper.Escape(VersionId)).Append('\n');
            sb.Append("Range=").Append(CheckpointHelper.Escape(Range)).Append('\n');
            sb.Append("ObjectSize=").Append(ObjectSize.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("ObjectLastModified=").Append(CheckpointHelper.Escape(ObjectLastModified)).Append('\n');
            sb.Append("ObjectETag=").Append(CheckpointHelper.Escape(ObjectETag)).Append('\n');
            sb.Append("FilePath=").Append(CheckpointHelper.Escape(FilePath)).Append('\n');
            sb.Append("PartSize=").Append(PartSize.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("Offset=").Append(Offset.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("Crc64=").Append(Crc64.ToString(CultureInfo.InvariantCulture)).Append('\n');
            return sb.ToString();
        }

        public void Load()
        {
            if (!Directory.Exists(CpDirPath))
                throw new IOException($"Invalid checkpoint dir, {CpDirPath}");

            if (!File.Exists(CpFilePath))
                return;

            if (!Valid())
            {
                Remove();
                return;
            }

            Loaded = true;
        }

        private bool Valid()
        {
            try
            {
                var text = File.ReadAllText(CpFilePath, Encoding.UTF8);
                var parsed = CheckpointHelper.Parse(text);
                if (parsed == null)
                    return false;

                var (magic, storedMd5, computedMd5, f) = parsed.Value;
                if (!string.Equals(Defaults.CheckpointMagic, magic, StringComparison.Ordinal))
                    return false;
                if (!string.Equals(storedMd5, computedMd5, StringComparison.Ordinal))
                    return false;

                if (!string.Equals(Get(f, "ObjectName"), ObjectName, StringComparison.Ordinal)) return false;
                if (!string.Equals(Get(f, "VersionId"), VersionId, StringComparison.Ordinal)) return false;
                if (!string.Equals(Get(f, "Range"), Range, StringComparison.Ordinal)) return false;
                if (GetLong(f, "ObjectSize") != ObjectSize) return false;
                if (!string.Equals(Get(f, "ObjectLastModified"), ObjectLastModified, StringComparison.Ordinal)) return false;
                if (!string.Equals(Get(f, "ObjectETag"), ObjectETag, StringComparison.Ordinal)) return false;
                if (!string.Equals(Get(f, "FilePath"), FilePath, StringComparison.Ordinal)) return false;
                if (GetLong(f, "PartSize") != PartSize) return false;

                var offset = GetLong(f, "Offset");
                var crc = GetULong(f, "Crc64");
                if (offset < 0)
                    return false;
                if (offset == 0 && crc != 0)
                    return false;

                long rOffset = 0;
                if (!string.IsNullOrEmpty(Range))
                {
                    var r = RangeUtils.ParseRange(Range);
                    if (r == null)
                        return false;
                    if (r.Value.IsSuffix)
                        rOffset = Math.Max(0, ObjectSize - r.Value.SuffixLength);
                    else
                        rOffset = r.Value.Start;
                }

                if (offset < rOffset)
                    return false;
                if ((offset - rOffset) % PartSize != 0)
                    return false;

                // valid destination path
                var downloadLen = offset - rOffset;
                if (downloadLen > 0)
                {
                    var fileInfo = new FileInfo(FilePath);
                    if (!fileInfo.Exists || fileInfo.Length < downloadLen)
                        return false;
                }

                if (VerifyData && crc != 0 && File.Exists(FilePath))
                {
                    var limit = offset - rOffset;
                    var hash = new HashCrc64(0UL);
                    using (var fs = File.OpenRead(FilePath))
                    {
                        var buffer = new byte[Defaults.DefaultCopyBufferSize];
                        long remaining = limit;
                        int n;
                        while (remaining > 0 &&
                               (n = fs.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining))) > 0)
                        {
                            hash.Update(buffer, 0, n);
                            remaining -= n;
                        }
                    }
                    var computed = BitConverter.ToUInt64(hash.Final(), 0);
                    if (computed != crc)
                        return false;
                }

                Offset = offset;
                Crc64 = crc;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dump()
        {
            var dataText = BuildDataText();
            var md5 = CheckpointHelper.Md5Hex(dataText);
            var sb = new StringBuilder();
            sb.Append("Magic=").Append(Defaults.CheckpointMagic).Append('\n');
            sb.Append("MD5=").Append(md5).Append('\n');
            sb.Append(dataText);
            File.WriteAllText(CpFilePath, sb.ToString(), new UTF8Encoding(false));
        }

        public void Remove()
        {
            try { if (File.Exists(CpFilePath)) File.Delete(CpFilePath); }
            catch { /* ignore */ }
        }

        private static string Get(Dictionary<string, string> f, string key) => f.TryGetValue(key, out var v) ? v : "";
        private static long GetLong(Dictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) && long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : 0;
        private static ulong GetULong(Dictionary<string, string> f, string key) =>
            f.TryGetValue(key, out var v) && ulong.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : 0;
    }
}
