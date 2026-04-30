namespace NurFlac.Storage;

/// <summary>
/// Composite implementation of <see cref="IStorageServiceFactory"/> that 
/// aggregates multiple factories to produce a <see cref="CompositeStorageService"/>.
/// </summary>
public class CompositeStorageFactory : IStorageServiceFactory
{
    private readonly IEnumerable<IStorageServiceFactory> _factories;

    public CompositeStorageFactory(IEnumerable<IStorageServiceFactory> factories)
    {
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
    }

    public IFileUploader CreateFileUploader() => CreateStorageService();
    public IDirectoryManager CreateDirectoryManager() => CreateStorageService();
    public IStorageDiagnostics CreateDiagnostics() => CreateStorageService();

    public IStorageService CreateStorageService()
    {
        var services = _factories.Select(f => f.CreateStorageService());
        return new CompositeStorageService(services);
    }
}
