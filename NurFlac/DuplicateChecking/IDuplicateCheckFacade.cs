using NurFlac.DuplicateChecking.Models;

namespace NurFlac.DuplicateChecking;

public interface IDuplicateCheckFacade
{
    Task<DuplicateCheckResult> CheckAsync(string filePath, CancellationToken cancellationToken = default);
    Task RegisterUploadedAsync(DuplicateCheckResult checkResult, string fileName, long uploadedByTelegramId, CancellationToken cancellationToken = default);
}
