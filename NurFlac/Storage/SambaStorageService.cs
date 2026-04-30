using System.Text;

namespace NurFlac.Storage;

public class SambaStorageService : StorageService
{
    private static readonly string DefaultBaseRoot = Path.Combine(Path.GetTempPath(), "NurFlac", "Samba");

    private readonly string _sharePath;
    private readonly string _storageRoot;

    public SambaStorageService(string sharePath, string? rootPath = null)
    {
        _sharePath = sharePath;
        _storageRoot = ResolveStorageRoot(sharePath, rootPath);
    }

    public override Task<bool> CreateDirectoryAsync(string folderPath)
    {
        try
        {
            var fullPath = BuildRemotePath(folderPath);
            Directory.CreateDirectory(fullPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public override async Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var targetDirectory = BuildRemotePath(folderPath);
            Directory.CreateDirectory(targetDirectory);

            var destinationPath = Path.Combine(targetDirectory, remoteFileName);

            await using var source = File.OpenRead(filePath);
            await using var destination = File.Create(destinationPath);
            await source.CopyToAsync(destination);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public override Task<bool> CheckConnectionAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_sharePath))
            {
                return Task.FromResult(false);
            }

            Directory.CreateDirectory(_storageRoot);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string BuildRemotePath(string folderPath)
    {
        var normalized = (folderPath ?? string.Empty)
            .Trim()
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        return string.IsNullOrWhiteSpace(normalized)
            ? _storageRoot
            : Path.Combine(_storageRoot, normalized);
    }

    private static string ResolveStorageRoot(string sharePath, string? rootPath = null)
    {
        if (string.IsNullOrWhiteSpace(sharePath))
        {
            var baseRoot = string.IsNullOrWhiteSpace(rootPath) ? DefaultBaseRoot : rootPath;
            return Path.Combine(baseRoot, "unknown-share");
        }

        if (Path.IsPathRooted(sharePath) && Directory.Exists(sharePath))
        {
            return sharePath;
        }

        var baseRootForRelative = string.IsNullOrWhiteSpace(rootPath) ? DefaultBaseRoot : rootPath;
        var safeShare = SanitizePathSegment(sharePath.Replace("\\", "/").Replace('/', '_'));
        return Path.Combine(baseRootForRelative, safeShare);
    }

    private static string SanitizePathSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            builder.Append(invalidChars.Contains(ch) ? '_' : ch);
        }

        return builder.ToString();
    }
}