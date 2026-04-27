namespace NurFlac.Validation;

/// <summary>
/// Extrinsic state passed into each validator. Holds per-request data that is not
/// shared between calls, as opposed to the intrinsic AudioFormat flyweight objects.
/// </summary>
public sealed class AudioFileContext
{
    public string FileName { get; }
    public string Extension { get; }
    public string? MimeType { get; }
    public string TelegramFileId { get; }

    /// <summary>
    /// Set after the file has been downloaded to a local temp path.
    /// Null until download completes — spectral validation skips when null.
    /// </summary>
    public string? LocalFilePath { get; set; }

    public AudioFileContext(
        string fileName,
        string extension,
        string? mimeType,
        string telegramFileId)
    {
        FileName = fileName;
        Extension = extension;
        MimeType = mimeType;
        TelegramFileId = telegramFileId;
    }
}
