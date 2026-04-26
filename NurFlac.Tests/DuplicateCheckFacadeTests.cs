using NurFlac.DuplicateChecking;
using NurFlac.DuplicateChecking.Models;

namespace NurFlac.Tests;

public class DuplicateCheckFacadeTests
{
    [Fact]
    public async Task CheckThenRegisterThenCheckAgain_DetectsDuplicate()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"nurflac-dup-{Guid.NewGuid():N}.db");
        var repository = new SqliteDuplicateFingerprintRepository(dbPath);
        var providers = new IAudioFingerprintProvider[]
        {
            new ConstantFingerprintProvider("fingerprint-123", "test-provider")
        };

        var facade = new DuplicateCheckFacade(providers, repository);

        var firstCheck = await facade.CheckAsync("file-a.flac");
        Assert.False(firstCheck.IsDuplicate);

        await facade.RegisterUploadedAsync(firstCheck, "file-a.flac", 42);

        var secondCheck = await facade.CheckAsync("file-b.flac");
        Assert.True(secondCheck.IsDuplicate);

    }

    private sealed class ConstantFingerprintProvider : IAudioFingerprintProvider
    {
        private readonly AudioFingerprint _fingerprint;

        public ConstantFingerprintProvider(string value, string providerName)
        {
            _fingerprint = new AudioFingerprint(value, providerName);
        }

        public Task<AudioFingerprint> CreateFingerprintAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_fingerprint);
        }
    }
}
