// ConcreteState: Idle — processes single uploads immediately; responds to /album-upload.
using NurFlac.Audio.Models;

namespace NurFlac.Album.States;

public sealed class IdleState : IAlbumState
{
    public string StateName => "Idle";

    public Task<string> HandleFileAsync(AlbumSession ctx, AudioFileContext file, CancellationToken ct)
    {
        // In Idle mode, single-file uploads are processed outside the album flow.
        return Task.FromResult("File received for single validation.");
    }

    public Task<string> HandleAlbumUploadCommandAsync(AlbumSession ctx, CancellationToken ct)
    {
        ctx.PendingFiles.Clear();
        ctx.TransitionTo(new AlbumUploadState());
        return Task.FromResult("Album session started. Send your audio files, then /album-done.");
    }

    public Task<string> HandleAlbumDoneCommandAsync(AlbumSession ctx, CancellationToken ct) =>
        Task.FromResult("No album session is active. Use /album-upload first.");
}
