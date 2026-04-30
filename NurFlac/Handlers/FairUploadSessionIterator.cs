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
        var allSessions = (await _caretaker.GetPendingSessionsAsync()).ToList();
        if (allSessions.Count == 0)
        {
            _buffer = new List<UploadSessionMemento>();
            return;
        }

        // Implementation of Fair Round-Robin Logic:
        // 1. Group by User
        var groups = allSessions
            .GroupBy(s => s.TelegramId)
            .Select(g => new Queue<UploadSessionMemento>(g.OrderBy(s => s.CreatedAt)))
            .ToList();

        var fairList = new List<UploadSessionMemento>();
        
        // 2. Interleave sessions from different users
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
