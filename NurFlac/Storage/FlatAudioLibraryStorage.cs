using NurFlac.Validation;

namespace NurFlac.Storage;

/// <summary>
/// Refined abstraction (Bridge) — uploads all audio files into the storage root
/// with no subfolder organization.
/// </summary>
public sealed class FlatAudioLibraryStorage : AudioLibraryStorage
{
    public FlatAudioLibraryStorage(IStorageService storage) : base(storage) { }

    public override Task<bool> UploadAudioAsync(AudioFileContext context, CancellationToken cancellationToken = default)
        => Storage.UploadFileAsync(context.LocalFilePath!, context.FileName, folderPath: string.Empty);
}
