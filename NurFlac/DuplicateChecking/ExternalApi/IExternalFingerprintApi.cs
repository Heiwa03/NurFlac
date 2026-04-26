namespace NurFlac.DuplicateChecking.ExternalApi;

public interface IExternalFingerprintApi
{
    Task<string> GenerateFingerprintAsync(string filePath, CancellationToken cancellationToken = default);
}
