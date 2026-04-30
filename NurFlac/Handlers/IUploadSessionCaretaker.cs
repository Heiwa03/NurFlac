using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

/// <summary>
/// The Caretaker interface: Responsible for saving and retrieving mementos.
/// </summary>
public interface IUploadSessionCaretaker
{
    Task SaveMementoAsync(UploadSessionMemento memento);
    Task<IEnumerable<UploadSessionMemento>> GetPendingSessionsAsync();
    Task RemoveMementoAsync(string sessionId);
}
