using NurFlac.Handlers.Models;
using Telegram.Bot.Types;

namespace NurFlac.Handlers;

public class UploadSessionQueue : IUploadSessionQueue
{
    private readonly IUploadSessionCaretaker _caretaker;
    private readonly SemaphoreSlim _signal = new(0);

    public UploadSessionQueue(IUploadSessionCaretaker caretaker)
    {
        _caretaker = caretaker;
    }

    public IUploadSessionIterator GetFairIterator()
    {
        return new FairUploadSessionIterator(_caretaker);
    }

    public async Task EnqueueAsync(Message message, long telegramId)
    {
        string fileId;
        string fileName;

        if (message.Audio is { } audio)
        {
            fileId = audio.FileId;
            fileName = audio.FileName ?? $"audio_{Guid.NewGuid():N}";
        }
        else if (message.Document is { } doc)
        {
            fileId = doc.FileId;
            fileName = doc.FileName ?? $"file_{Guid.NewGuid():N}";
        }
        else return;

        var sessionId = Guid.NewGuid().ToString("N");
        var memento = new UploadSessionMemento(
            sessionId, 
            telegramId, 
            message.Chat.Id, 
            fileId, 
            fileName, 
            null, 
            UploadStatus.Started, 
            DateTime.UtcNow);

        await _caretaker.SaveMementoAsync(memento);
        NotifyChange();
    }

    public void NotifyChange()
    {
        try { _signal.Release(); } catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Helper for the background processor to wait for new work
    /// </summary>
    public async Task WaitForWorkAsync(CancellationToken ct)
    {
        await _signal.WaitAsync(ct);
    }
}
