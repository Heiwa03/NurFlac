using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

/// <summary>
/// The Iterator interface for traversing upload sessions.
/// </summary>
public interface IUploadSessionIterator
{
    Task<bool> MoveNextAsync();
    UploadSessionMemento Current { get; }
}
