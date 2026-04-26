using System.Text;

namespace NurFlac.Storage;

public class SftpStorageService : StorageService
{
    private static readonly string BaseRoot = Path.Combine(Path.GetTempPath(), "NurFlac", "Sftp");

    private readonly string _host;
    private readonly string _username;
    private readonly string _storageRoot;

    public SftpStorageService(string host, string username)
    {
        _host = host;
        _username = username;

        var safeHost = SanitizePathSegment(host);
        var safeUser = SanitizePathSegment(username);
        _storageRoot = Path.Combine(BaseRoot, safeHost, safeUser);
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
            if (string.IsNullOrWhiteSpace(_host) || string.IsNullOrWhiteSpace(_username))
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

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            builder.Append(invalidChars.Contains(ch) ? '_' : ch);
        }

        return builder.ToString();
    }
}