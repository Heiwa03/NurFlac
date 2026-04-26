namespace NurFlac.Storage;

/// <summary>
/// Optional shared base class for storage providers.
/// </summary>
public abstract class StorageService : IStorageService
{
    public abstract Task<bool> CreateDirectoryAsync(string folderPath);
    public abstract Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath);
    public abstract Task<bool> CheckConnectionAsync();
}