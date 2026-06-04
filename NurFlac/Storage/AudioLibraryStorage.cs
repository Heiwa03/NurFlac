using NurFlac.Audio.Models;

namespace NurFlac.Storage;

/// <summary>
/// Bridge abstraction — defines how audio files are organized and uploaded to the library.
/// Holds a reference to <see cref="IStorageService"/> (the implementor) so that the
/// organization scheme and the storage backend can vary independently.
/// </summary>
public abstract class AudioLibraryStorage
{
    protected readonly IStorageService Storage;

    protected AudioLibraryStorage(IStorageService storage)
    {
        Storage = storage;
    }

    /// <summary>
    /// Uploads the validated audio file according to this storage's organization scheme.
    /// Requires <see cref="AudioFileContext.LocalFilePath"/> to be set.
    /// </summary>
    public abstract Task<bool> UploadAudioAsync(AudioFileContext context, CancellationToken cancellationToken = default);
}
