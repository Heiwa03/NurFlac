namespace NurFlac.Audio.Models;

public sealed class AudioFormat
{
    public string Extension { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsLossless { get; init; }
}
