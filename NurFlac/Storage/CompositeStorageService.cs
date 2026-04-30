namespace NurFlac.Storage;

/// <summary>
/// Composite implementation of <see cref="IStorageService"/> that allows 
/// performing storage operations across multiple destinations simultaneously.
/// </summary>
public class CompositeStorageService : IStorageService
{
    private readonly IEnumerable<IStorageService> _children;

    public CompositeStorageService(IEnumerable<IStorageService> children)
    {
        _children = children ?? throw new ArgumentNullException(nameof(children));
    }

    public async Task<bool> CheckConnectionAsync()
    {
        var tasks = _children.Select(c => c.CheckConnectionAsync());
        var results = await Task.WhenAll(tasks);
        // Returns true only if all destinations are reachable
        return results.All(r => r);
    }

    public async Task<bool> CreateDirectoryAsync(string folderPath)
    {
        var tasks = _children.Select(c => c.CreateDirectoryAsync(folderPath));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath)
    {
        var tasks = _children.Select(c => c.UploadFileAsync(filePath, remoteFileName, folderPath));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }
}
