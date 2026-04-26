using Microsoft.Extensions.Logging;

namespace NurFlac.Storage;

public class WebDavStorageFactory : IStorageServiceFactory
{
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly ILoggerFactory _loggerFactory;

    public WebDavStorageFactory(string baseUrl, string username, string password, ILoggerFactory loggerFactory)
    {
        _baseUrl = baseUrl;
        _username = username;
        _password = password;
        _loggerFactory = loggerFactory;
    }

    public IFileUploader CreateFileUploader() => CreateService();
    public IDirectoryManager CreateDirectoryManager() => CreateService();
    public IStorageDiagnostics CreateDiagnostics() => CreateService();
    public IStorageService CreateStorageService() => CreateService();

    private WebDavStorageService CreateService() =>
        new(_baseUrl, _username, _password, _loggerFactory.CreateLogger<WebDavStorageService>());
}