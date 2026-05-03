using NurFlac.Handlers.Models;

namespace NurFlac.Handlers;

public class FairUploadSessionIterator : IUploadSessionIterator
{
    private readonly IUploadSessionCaretaker _caretaker;
    private List<UploadSessionMemento> _buffer = new();
    private int _currentIndex = -1;

    public FairUploadSessionIterator(IUploadSessionCaretaker caretaker)
    {
        _caretaker = caretaker;
    }

    public UploadSessionMemento Current => _buffer[_currentIndex];

    public async Task<bool> MoveNextAsync()
    {
        // If we are at the end of the current buffer, try to refresh from database
        if (_currentIndex >= _buffer.Count - 1)
        {
            await RefreshBufferAsync();
            _currentIndex = 0;
        }
        else
        {
            _currentIndex++;
        }

        return _buffer.Count > 0 && _currentIndex < _buffer.Count;
    }

    private async Task RefreshBufferAsync()
    {
        // FILTER: Only fetch sessions that are NOT already in the Processing state (State Pattern)
        var allSessions = (await _caretaker.GetPendingSessionsAsync())
            .Where(s => s.Status != UploadStatus.Processing)
            .ToList();

        if (allSessions.Count == 0)
        {
            _buffer = new List<UploadSessionMemento>();
            return;
        }

        // Implementation of Fair Round-Robin Logic:
        var groups = allSessions
            .GroupBy(s => s.TelegramId)
            .Select(g => new Queue<UploadSessionMemento>(g.OrderBy(s => s.CreatedAt)))
            .ToList();

        var fairList = new List<UploadSessionMemento>();
        
        while (groups.Any(g => g.Count > 0))
        {
            foreach (var group in groups)
            {
                if (group.Count > 0)
                {
                    fairList.Add(group.Dequeue());
                }
            }
        }

        _buffer = fairList;
    }
}
