using NurFlac.Audio.Facade;
using NurFlac.Audio.Models;
using NurFlac.Ledger;
using NurFlac.Storage;

namespace NurFlac.Album;

public sealed class AlbumSessionManager(AudioPipelineFacade pipeline, LedgerService ledger, AudioLibraryStorage library)
{
    private readonly Dictionary<long, AlbumSession> _sessions = [];
    private readonly Lock _lock = new();

    private AlbumSession GetOrCreate(long telegramId)
    {
        lock (_lock)
        {
            if (!_sessions.TryGetValue(telegramId, out var session))
            {
                session = new AlbumSession(telegramId, pipeline, ledger, library);
                _sessions[telegramId] = session;
            }
            return session;
        }
    }

    public Task<string> HandleFileAsync(long telegramId, AudioFileContext file, CancellationToken ct) =>
        GetOrCreate(telegramId).HandleFileAsync(file, ct);

    public Task<string> StartAlbumAsync(long telegramId, CancellationToken ct) =>
        GetOrCreate(telegramId).HandleAlbumUploadAsync(ct);

    public Task<string> FinalizeAlbumAsync(long telegramId, CancellationToken ct) =>
        GetOrCreate(telegramId).HandleAlbumDoneAsync(ct);
}
