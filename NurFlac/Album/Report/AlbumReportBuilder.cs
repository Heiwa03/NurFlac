// ConcreteBuilder — thread-safe step-by-step assembly of an AlbumReport.
namespace NurFlac.Album.Report;

public sealed class AlbumReportBuilder(long telegramId) : IAlbumReportBuilder
{
    // Use the new .lock type (C# 13 / .NET 9) instead of a plain object lock.
    private readonly Lock _lock     = new();
    private readonly List<string>                   _accepted = [];
    private readonly List<(string, string)>         _rejected = [];

    public IAlbumReportBuilder AddSuccess(string fileName)
    {
        lock (_lock) _accepted.Add(fileName);
        return this;
    }

    public IAlbumReportBuilder AddFailure(string fileName, string reason)
    {
        lock (_lock) _rejected.Add((fileName, reason));
        return this;
    }

    public AlbumReport Build()
    {
        lock (_lock)
        {
            return new AlbumReport
            {
                TelegramId  = telegramId,
                GeneratedAt = DateTime.UtcNow,
                Accepted    = [.._accepted],
                Rejected    = [.._rejected]
            };
        }
    }
}
