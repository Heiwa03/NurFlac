// Context class for the State pattern — holds current state and injected services.
using NurFlac.Album.States;
using NurFlac.Audio.Facade;
using NurFlac.Audio.Models;
using NurFlac.Ledger;
using NurFlac.Storage;

namespace NurFlac.Album;

public sealed class AlbumSession
{
    public long                   TelegramId   { get; }
    public List<AudioFileContext> PendingFiles { get; } = [];

    // Services available to states that need them.
    internal readonly AudioPipelineFacade Pipeline;
    internal readonly LedgerService       Ledger;
    internal readonly AudioLibraryStorage Library;

    private IAlbumState _state;

    public AlbumSession(long telegramId, AudioPipelineFacade pipeline, LedgerService ledger, AudioLibraryStorage library)
    {
        TelegramId = telegramId;
        Pipeline   = pipeline;
        Ledger     = ledger;
        Library    = library;
        _state     = new IdleState();
    }

    public string CurrentStateName => _state.StateName;
    public void   TransitionTo(IAlbumState newState) => _state = newState;

    public Task<string> HandleFileAsync(AudioFileContext file, CancellationToken ct) =>
        _state.HandleFileAsync(this, file, ct);

    public Task<string> HandleAlbumUploadAsync(CancellationToken ct) =>
        _state.HandleAlbumUploadCommandAsync(this, ct);

    public Task<string> HandleAlbumDoneAsync(CancellationToken ct) =>
        _state.HandleAlbumDoneCommandAsync(this, ct);
}
