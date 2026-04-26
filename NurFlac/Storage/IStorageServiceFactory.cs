namespace NurFlac.Storage;

/// <summary>
/// Abstract Factory pattern — creates a family of related storage objects
/// (uploader, directory manager, diagnostics) without specifying concrete classes.
/// </summary>
public interface IStorageServiceFactory
{
    IFileUploader CreateFileUploader();
    IDirectoryManager CreateDirectoryManager();
    IStorageDiagnostics CreateDiagnostics();

    /// <summary>Convenience: returns the full aggregate service.</summary>
    IStorageService CreateStorageService();
}