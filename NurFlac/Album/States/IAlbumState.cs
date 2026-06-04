// ============================================================
// PATTERN: State (Behavioral)
// Role   : State interface — defines the operations that vary
//          by the current album-upload state of a user session.
// ============================================================
using NurFlac.Audio.Models;

namespace NurFlac.Album.States;

public interface IAlbumState
{
    string StateName { get; }
    Task<string> HandleFileAsync(AlbumSession ctx, AudioFileContext file, CancellationToken ct);
    Task<string> HandleAlbumUploadCommandAsync(AlbumSession ctx, CancellationToken ct);
    Task<string> HandleAlbumDoneCommandAsync(AlbumSession ctx,   CancellationToken ct);
}
