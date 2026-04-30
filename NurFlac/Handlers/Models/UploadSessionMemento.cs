namespace NurFlac.Handlers.Models;

/// <summary>
/// The Memento: Represents the captured state of an in-progress upload session.
/// </summary>
public record UploadSessionMemento(
    string SessionId,
    long TelegramId,
    long ChatId,
    string FileId,
    string FileName,
    string? LocalFilePath,
    UploadStatus Status,
    DateTime CreatedAt
);

public enum UploadStatus
{
    Started,
    Downloaded,
    Validated,
    Completed,
    Failed
}
