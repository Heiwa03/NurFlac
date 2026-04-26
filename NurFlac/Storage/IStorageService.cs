namespace NurFlac.Storage;

/// <summary>
/// Aggregate interface — inherits all segregated storage capabilities.
/// </summary>
public interface IStorageService : IFileUploader, IDirectoryManager, IStorageDiagnostics
{
}
