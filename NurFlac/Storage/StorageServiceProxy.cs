using System.Diagnostics;

namespace NurFlac.Storage;

/// <summary>
/// Proxy (Logging Proxy) — transparently wraps <see cref="IStorageService"/> and
/// records every operation with its result and elapsed time.
/// Registered as the <see cref="IStorageService"/> singleton in DI; all callers
/// receive this proxy without being aware of it.
/// </summary>
public sealed class StorageServiceProxy : IStorageService
{
    private readonly IStorageService _inner;
    private readonly ILogger<StorageServiceProxy> _logger;

    public StorageServiceProxy(IStorageService inner, ILogger<StorageServiceProxy> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.UploadFileAsync(filePath, remoteFileName, folderPath);
        _logger.LogInformation(
            "Storage.UploadFileAsync({RemoteFileName}, folder={Folder}) → {Result} in {Ms}ms",
            remoteFileName, folderPath, result, sw.ElapsedMilliseconds);
        return result;
    }

    public async Task<bool> CreateDirectoryAsync(string folderPath)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.CreateDirectoryAsync(folderPath);
        _logger.LogInformation(
            "Storage.CreateDirectoryAsync({Folder}) → {Result} in {Ms}ms",
            folderPath, result, sw.ElapsedMilliseconds);
        return result;
    }

    public async Task<bool> CheckConnectionAsync()
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.CheckConnectionAsync();
        _logger.LogInformation(
            "Storage.CheckConnectionAsync() → {Result} in {Ms}ms",
            result, sw.ElapsedMilliseconds);
        return result;
    }
}
