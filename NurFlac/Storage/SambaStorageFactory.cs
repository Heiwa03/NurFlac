namespace NurFlac.Storage;

public class SambaStorageFactory : IStorageServiceFactory
{
    private readonly string _sharePath;
    private readonly string? _rootPath;

    public SambaStorageFactory(string sharePath, string? rootPath = null)
    {
        _sharePath = sharePath;
        _rootPath = rootPath;
    }

    public IFileUploader CreateFileUploader() => CreateService();
    public IDirectoryManager CreateDirectoryManager() => CreateService();
    public IStorageDiagnostics CreateDiagnostics() => CreateService();
    public IStorageService CreateStorageService() => CreateService();

    private SambaStorageService CreateService() => new(_sharePath, _rootPath);
}