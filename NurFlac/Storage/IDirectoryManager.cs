namespace NurFlac.Storage;

public interface IDirectoryManager
{
    Task<bool> CreateDirectoryAsync(string folderPath);
}