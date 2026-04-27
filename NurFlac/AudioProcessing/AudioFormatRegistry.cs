namespace NurFlac.AudioProcessing;

/// <summary>
/// Flyweight factory — creates and caches one <see cref="AudioFormat"/> instance per format.
/// Callers share the same flyweight objects; only extrinsic state (the file path, content)
/// differs between requests.
/// </summary>
public sealed class AudioFormatRegistry
{
    private readonly IReadOnlyDictionary<string, AudioFormat> _byExtension;
    private readonly IReadOnlyDictionary<string, AudioFormat> _byMimeType;
    private readonly IReadOnlyList<AudioFormat> _all;

    public AudioFormatRegistry()
    {
        var formats = new[]
        {
            new AudioFormat("flac", "FLAC",
                [".flac"],
                ["audio/flac", "audio/x-flac"],
                isLossless: true),

            new AudioFormat("wav", "WAV",
                [".wav"],
                ["audio/wav", "audio/x-wav", "audio/wave"],
                isLossless: true),

            new AudioFormat("alac", "ALAC",
                [".alac", ".m4a"],
                ["audio/alac", "audio/x-m4a", "audio/mp4"],
                isLossless: true),

            new AudioFormat("aiff", "AIFF",
                [".aiff", ".aif"],
                ["audio/aiff", "audio/x-aiff"],
                isLossless: true),

            new AudioFormat("mp3", "MP3",
                [".mp3"],
                ["audio/mpeg", "audio/mp3"],
                isLossless: false),

            new AudioFormat("aac", "AAC",
                [".aac"],
                ["audio/aac"],
                isLossless: false),

            new AudioFormat("ogg", "OGG Vorbis",
                [".ogg"],
                ["audio/ogg"],
                isLossless: false),

            new AudioFormat("opus", "Opus",
                [".opus"],
                ["audio/opus"],
                isLossless: false),
        };

        _all = formats;

        var byExt = new Dictionary<string, AudioFormat>(StringComparer.OrdinalIgnoreCase);
        var byMime = new Dictionary<string, AudioFormat>(StringComparer.OrdinalIgnoreCase);

        foreach (var fmt in formats)
        {
            foreach (var ext in fmt.Extensions)
                byExt[ext] = fmt;

            foreach (var mime in fmt.MimeTypes)
                byMime[mime] = fmt;
        }

        _byExtension = byExt;
        _byMimeType = byMime;
    }

    /// <summary>Returns the shared flyweight for the given file extension, or null if unknown.</summary>
    public AudioFormat? GetByExtension(string extension) =>
        _byExtension.TryGetValue(extension, out var fmt) ? fmt : null;

    /// <summary>Returns the shared flyweight for the given MIME type, or null if unknown.</summary>
    public AudioFormat? GetByMimeType(string mimeType) =>
        _byMimeType.TryGetValue(mimeType, out var fmt) ? fmt : null;

    public IReadOnlyList<AudioFormat> GetAll() => _all;

    public IReadOnlyList<AudioFormat> GetAllLossless() =>
        _all.Where(f => f.IsLossless).ToList();
}
