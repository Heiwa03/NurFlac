using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlac.Storage;
using NurFlac.Validation;
using NurFlac.Handlers.Models;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class SingleAudioUploadCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly AudioLibraryStorage _audioLibraryStorage;
    private readonly ILosslessAudioValidator _validator;
    private readonly ILogger<SingleAudioUploadCommand> _logger;
    private readonly IUploadSessionCaretaker _caretaker;
    private readonly string _botToken;
    private readonly string? _localPath;
    private readonly string? _nginxUrl;

    public SingleAudioUploadCommand(
        ITelegramBotClient botClient,
        AudioLibraryStorage audioLibraryStorage,
        ILosslessAudioValidator validator,
        ILogger<SingleAudioUploadCommand> logger,
        IConfiguration configuration,
        IUploadSessionCaretaker caretaker)
    {
        _botClient = botClient;
        _audioLibraryStorage = audioLibraryStorage;
        _validator = validator;
        _logger = logger;
        _caretaker = caretaker;
        _botToken = configuration["TelegramBot:Token"] ?? string.Empty;
        _localPath = configuration["TelegramBot:LocalApi:LocalPath"];
        _nginxUrl = configuration["TelegramBot:LocalApi:NginxUrl"];
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        string fileId;
        string fileName;
        string? mimeType;

        if (message.Audio is { } audio)
        {
            fileId = audio.FileId;
            fileName = audio.FileName ?? $"audio_{Guid.NewGuid():N}";
            mimeType = audio.MimeType;
        }
        else if (message.Document is { } doc)
        {
            fileId = doc.FileId;
            fileName = doc.FileName ?? $"file_{Guid.NewGuid():N}";
            mimeType = doc.MimeType;
        }
        else
        {
            return;
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var memento = new UploadSessionMemento(sessionId, user.TelegramId, message.Chat.Id, fileId, fileName, null, UploadStatus.Started, DateTime.UtcNow);
        await _caretaker.SaveMementoAsync(memento);

        await ProcessUploadAsync(memento, mimeType);
    }

    public async Task ResumeSessionAsync(UploadSessionMemento memento)
    {
        _logger.LogInformation("Resuming upload session {SessionId} for file {FileName}", memento.SessionId, memento.FileName);
        await _botClient.SendMessage(memento.ChatId, $"Resuming interrupted upload for \"{memento.FileName}\"...");
        await ProcessUploadAsync(memento, null);
    }

    private async Task ProcessUploadAsync(UploadSessionMemento memento, string? mimeType)
    {
        var extension = Path.GetExtension(memento.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) && mimeType is not null)
            extension = MimeToExtension(mimeType);

        var context = new AudioFileContext(memento.FileName, extension, mimeType, memento.FileId);
        
        if (memento.Status == UploadStatus.Started)
        {
            var preCheck = await _validator.ValidateAsync(context);
            if (!preCheck.IsValid)
            {
                await _botClient.SendMessage(memento.ChatId, $"File rejected: {preCheck.RejectionReason}");
                await _caretaker.RemoveMementoAsync(memento.SessionId);
                return;
            }

            await _botClient.SendMessage(memento.ChatId, "Format check passed. Downloading for analysis...");

            var tempPath = Path.Combine(Path.GetTempPath(), $"nurflac_{memento.SessionId}{extension}");
            try
            {
                var tgFile = await _botClient.GetFile(memento.FileId);
                await using (var fs = File.OpenWrite(tempPath))
                    await DownloadToStreamAsync(tgFile.FilePath!, fs);

                memento = memento with { LocalFilePath = tempPath, Status = UploadStatus.Downloaded };
                await _caretaker.SaveMementoAsync(memento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download failed for {FileName}", memento.FileName);
                await _botClient.SendMessage(memento.ChatId, "Download failed. Will retry later.");
                return;
            }
        }

        if (memento.Status == UploadStatus.Downloaded || memento.Status == UploadStatus.Validated)
        {
            context.LocalFilePath = memento.LocalFilePath;
            
            if (memento.Status == UploadStatus.Downloaded)
            {
                var fullCheck = await _validator.ValidateAsync(context);
                if (!fullCheck.IsValid)
                {
                    await _botClient.SendMessage(memento.ChatId, $"File rejected: {fullCheck.RejectionReason}");
                    if (File.Exists(memento.LocalFilePath)) File.Delete(memento.LocalFilePath);
                    await _caretaker.RemoveMementoAsync(memento.SessionId);
                    return;
                }
                
                memento = memento with { Status = UploadStatus.Validated };
                await _caretaker.SaveMementoAsync(memento);
            }

            try
            {
                var uploaded = await _audioLibraryStorage.UploadAudioAsync(context);
                if (uploaded)
                {
                    await _botClient.SendMessage(memento.ChatId, $"\"{memento.FileName}\" accepted and uploaded to the library.");
                    await _caretaker.RemoveMementoAsync(memento.SessionId);
                }
                else
                {
                    await _botClient.SendMessage(memento.ChatId, "Upload to storage failed. Will retry later.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed for {FileName}", memento.FileName);
                await _botClient.SendMessage(memento.ChatId, "Upload failed. Will retry later.");
            }
            finally
            {
                if (memento.Status == UploadStatus.Completed && File.Exists(memento.LocalFilePath))
                    File.Delete(memento.LocalFilePath);
            }
        }
    }

    private static readonly HttpClient _httpClient = new HttpClient();

    private async Task DownloadToStreamAsync(string filePath, Stream destination)
    {
        if (!string.IsNullOrEmpty(_localPath) && !string.IsNullOrEmpty(_nginxUrl) && filePath.StartsWith(_localPath))
        {
            string downloadUrl = filePath.Replace(_localPath, _nginxUrl);
            _logger.LogInformation("Local mode Nginx download: {Url}", downloadUrl);
            using var responseStream = await _httpClient.GetStreamAsync(downloadUrl);
            await responseStream.CopyToAsync(destination);
            return;
        }
        await _botClient.DownloadFile(filePath, destination);
    }

    private static string MimeToExtension(string mimeType) => mimeType switch
    {
        "audio/flac" or "audio/x-flac" => ".flac",
        "audio/wav" or "audio/x-wav" or "audio/wave" => ".wav",
        "audio/aiff" or "audio/x-aiff" => ".aiff",
        "audio/alac" or "audio/x-m4a" or "audio/mp4" => ".m4a",
        "audio/mpeg" or "audio/mp3" => ".mp3",
        "audio/aac" => ".aac",
        "audio/ogg" => ".ogg",
        "audio/opus" => ".opus",
        _ => string.Empty
    };
}
