namespace NurFlac.Audio.Models;

public sealed class AudioFileContext
{
    public AudioFileContext(string fileName, string extension, string? mimeType, string fileId)
    {
        FileName  = fileName;
        Extension = extension.ToLowerInvariant();
        MimeType  = mimeType;
        FileId    = fileId;
    }

    public string  FileName      { get; }
    public string  Extension     { get; }
    public string? MimeType      { get; }
    public string  FileId        { get; }
    public string? LocalFilePath { get; set; }
}
