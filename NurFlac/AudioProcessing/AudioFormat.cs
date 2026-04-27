namespace NurFlac.AudioProcessing;

/// <summary>
/// Flyweight — holds intrinsic (shared, immutable) state for an audio format.
/// One instance per format is maintained by <see cref="AudioFormatRegistry"/>.
/// Extrinsic state (the specific file being processed) is passed by callers at runtime.
/// </summary>
public sealed class AudioFormat
{
    public string Id { get; }
    public string DisplayName { get; }
    public IReadOnlyList<string> Extensions { get; }
    public IReadOnlyList<string> MimeTypes { get; }
    public bool IsLossless { get; }

    internal AudioFormat(
        string id,
        string displayName,
        string[] extensions,
        string[] mimeTypes,
        bool isLossless)
    {
        Id = id;
        DisplayName = displayName;
        Extensions = extensions;
        MimeTypes = mimeTypes;
        IsLossless = isLossless;
    }
}
