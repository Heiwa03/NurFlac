namespace NurFlac.Storage;

public interface IStorageDiagnostics
{
    Task<bool> CheckConnectionAsync();
}