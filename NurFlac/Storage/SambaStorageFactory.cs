namespace NurFlac.Storage;

public class SambaStorageFactory : IStorageServiceFactory
{
    private readonly string _sharePath;

    public SambaStorageFactory(string sharePath)
    {
        _sharePath = sharePath;
    }

    public IFileUploader CreateFileUploader() => CreateService();
    public IDirectoryManager CreateDirectoryManager() => CreateService();
    public IStorageDiagnostics CreateDiagnostics() => CreateService();
    public IStorageService CreateStorageService() => CreateService();

    private SambaStorageService CreateService() => new(_sharePath);
}