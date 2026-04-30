using NurFlac.Handlers.Models;
using Telegram.Bot.Types;

namespace NurFlac.Handlers;

/// <summary>
/// The Aggregate interface that provides the Iterator.
/// </summary>
public interface IUploadSessionQueue
{
    IUploadSessionIterator GetFairIterator();
    Task EnqueueAsync(Message message, long telegramId);
    void NotifyChange();
    Task WaitForWorkAsync(CancellationToken ct);
}
