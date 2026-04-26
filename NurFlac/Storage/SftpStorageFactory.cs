namespace NurFlac.Storage;

public class SftpStorageFactory : IStorageServiceFactory
{
    private readonly string _host;
    private readonly string _username;

    public SftpStorageFactory(string host, string username)
    {
        _host = host;
        _username = username;
    }

    public IFileUploader CreateFileUploader() => CreateService();
    public IDirectoryManager CreateDirectoryManager() => CreateService();
    public IStorageDiagnostics CreateDiagnostics() => CreateService();
    public IStorageService CreateStorageService() => CreateService();

    private SftpStorageService CreateService() => new(_host, _username);
}