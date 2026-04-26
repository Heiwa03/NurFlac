namespace NurFlac.Storage;

public interface IFileUploader
{
    Task<bool> UploadFileAsync(string filePath, string remoteFileName, string folderPath);
}