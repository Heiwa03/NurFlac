namespace NurFlac.Storage;

public class SftpStorageFactory : IStorageServiceFactory
{
    private readonly string _host;
    private readonly string _username;
    private readonly string? _rootPath;

    public SftpStorageFactory(string host, string username, string? rootPath = null)
    {
        _host = host;
        _username = username;
        _rootPath = rootPath;
    }

    public IFileUploader CreateFileUploader() => CreateService();
    public IDirectoryManager CreateDirectoryManager() => CreateService();
    public IStorageDiagnostics CreateDiagnostics() => CreateService();
    public IStorageService CreateStorageService() => CreateService();

    private SftpStorageService CreateService() => new(_host, _username, _rootPath);
}