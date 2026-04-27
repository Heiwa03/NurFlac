using NurFlac.AudioProcessing;
using NurFlac.Validation;

namespace NurFlac.Storage;

/// <summary>
/// Refined abstraction (Bridge) — organizes audio files into format-named subfolders
/// (e.g. FLAC/, WAV/, ALAC/) resolved via the <see cref="AudioFormatRegistry"/> flyweight.
/// </summary>
public sealed class OrganizedAudioLibraryStorage : AudioLibraryStorage
{
    private readonly AudioFormatRegistry _registry;

    public OrganizedAudioLibraryStorage(IStorageService storage, AudioFormatRegistry registry)
        : base(storage)
    {
        _registry = registry;
    }

    public override Task<bool> UploadAudioAsync(AudioFileContext context, CancellationToken cancellationToken = default)
    {
        var format = _registry.GetByExtension(context.Extension);
        var folder = format?.DisplayName.ToUpperInvariant() ?? "UNKNOWN";
        return Storage.UploadFileAsync(context.LocalFilePath!, context.FileName, folder);
    }
}
