using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebDav;

namespace NurFlac.Storage;

public class WebDavStorageService : StorageService
{
    private readonly string _baseUrl;
    private readonly WebDavClient _client;
    private readonly ILogger<WebDavStorageService> _logger;

    public WebDavStorageService(string baseUrl, string username, string password, ILogger<WebDavStorageService> logger)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _logger = logger;

        var clientParams = new WebDavClientParams
        {
            BaseAddress = new Uri(_baseUrl),
            Credentials = new System.Net.NetworkCredential(username, password)
        };

        _client = new WebDavClient(clientParams);
    }

    public override async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var result = await _client.Propfind(_baseUrl);
            _logger.LogInformation("WebDAV connection check: {StatusCode}", result.StatusCode);
            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebDAV connection check failed");
            return false;
        }
    }

    public override async Task<bool> CreateDirectoryAsync(string folderPath)
    {
        try
        {
            var fullPath = $"{_baseUrl}/{folderPath.TrimStart('/')}";
            var result = await _client.Mkcol(fullPath);
            _logger.LogInformation("WebDAV MKCOL {Path}: {StatusCode}", fullPath, result.StatusCode);
            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebDAV CreateDirectory failed for {FolderPath}", folderPath);
            return false;
        }
    }

    public override async Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
    {
        try
        {
            var remotePath = string.IsNullOrEmpty(folderPath)
                ? $"{_baseUrl}/{remoteFileName}"
                : $"{_baseUrl}/{folderPath.TrimStart('/')}/{remoteFileName}";

            await using var fileStream = File.OpenRead(filePath);
            var result = await _client.PutFile(remotePath, fileStream);

            _logger.LogInformation("WebDAV PUT {Path}: {StatusCode}", remotePath, result.StatusCode);
            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebDAV upload failed for {FilePath} -> {RemoteFileName}", filePath, remoteFileName);
            return false;
        }
    }
}
