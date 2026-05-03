using Telegram.Bot;
using NurFlac.Storage;
using NurFlac.Validation;
using NurFlac.Handlers.Models;
using NurFlac.UserModeration.Mediator;
using NurFlac.UserModeration.Violations;
using NurFlac.UserModeration.States;
using NurFlac.UserManagement;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class SingleAudioUploadCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly AudioLibraryStorage _audioLibraryStorage;
    private readonly ILosslessAudioValidator _validator;
    private readonly ILogger<SingleAudioUploadCommand> _logger;
    private readonly IUploadSessionCaretaker _caretaker;
    private readonly IUploadSessionQueue _queue;
    private readonly IModerationMediator _moderationMediator;
    private readonly IUserService _userService;
    private readonly string _botToken;
    private readonly string? _localPath;
    private readonly string? _nginxUrl;

    public SingleAudioUploadCommand(
        ITelegramBotClient botClient,
        AudioLibraryStorage audioLibraryStorage,
        ILosslessAudioValidator validator,
        ILogger<SingleAudioUploadCommand> logger,
        IConfiguration configuration,
        IUploadSessionCaretaker caretaker,
        IUploadSessionQueue queue,
        IModerationMediator moderationMediator,
        IUserService userService)
    {
        _botClient = botClient;
        _audioLibraryStorage = audioLibraryStorage;
        _validator = validator;
        _logger = logger;
        _caretaker = caretaker;
        _queue = queue;
        _moderationMediator = moderationMediator;
        _userService = userService;
        _botToken = configuration["TelegramBot:Token"] ?? string.Empty;
        _localPath = configuration["TelegramBot:LocalApi:LocalPath"];
        _nginxUrl = configuration["TelegramBot:LocalApi:NginxUrl"];
    }

    public Task ExecuteAsync(Telegram.Bot.Types.Message message, User user) => Task.CompletedTask;

    public async Task ResumeSessionAsync(UploadSessionMemento memento)
    {
        _logger.LogInformation("Processing upload session {SessionId} for file {FileName} (Status: {Status})", 
            memento.SessionId, memento.FileName, memento.Status);
        
        await ProcessUploadAsync(memento, null);
    }

    private async Task ProcessUploadAsync(UploadSessionMemento memento, string? mimeType)
    {
        var extension = Path.GetExtension(memento.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) && mimeType is not null)
            extension = MimeToExtension(mimeType);

        var context = new AudioFileContext(memento.FileName, extension, mimeType, memento.FileId);
        
        var user = await _userService.GetOrCreateUserAsync(memento.TelegramId);

        // STATE PATTERN: Guard check (redundant but safe for background processing)
        IUserState state = user.Status switch
        {
            UserManagement.Entities.UserStatus.TimedOut => new TimedOutState(user.TimeoutUntil ?? DateTime.MinValue),
            UserManagement.Entities.UserStatus.Blacklisted => new BannedState(),
            _ => new ActiveState()
        };

        if (!state.CanUpload())
        {
            _logger.LogWarning("Upload task for {UserId} aborted: User is {Status}", memento.TelegramId, user.Status);
            await _caretaker.RemoveMementoAsync(memento.SessionId);
            return;
        }

        try 
        {
            if (memento.Status == UploadStatus.Started || memento.Status == UploadStatus.Processing)
            {
                var preCheck = await _validator.ValidateAsync(context);
                if (!preCheck.IsValid)
                {
                    await HandleRejectionAsync(memento, user, preCheck);
                    return;
                }

                var tempPath = Path.Combine(Path.GetTempPath(), $"nurflac_{memento.SessionId}{extension}");
                var tgFile = await _botClient.GetFile(memento.FileId);
                await using (var fs = File.OpenWrite(tempPath))
                    await DownloadToStreamAsync(tgFile.FilePath!, fs);

                memento = memento with { LocalFilePath = tempPath, Status = UploadStatus.Downloaded };
                await _caretaker.SaveMementoAsync(memento);
            }

            if (memento.Status == UploadStatus.Downloaded || memento.Status == UploadStatus.Validated)
            {
                context.LocalFilePath = memento.LocalFilePath;
                
                if (memento.Status == UploadStatus.Downloaded)
                {
                    var fullCheck = await _validator.ValidateAsync(context);
                    if (!fullCheck.IsValid)
                    {
                        if (File.Exists(memento.LocalFilePath)) File.Delete(memento.LocalFilePath);
                        await HandleRejectionAsync(memento, user, fullCheck);
                        return;
                    }
                    
                    memento = memento with { Status = UploadStatus.Validated };
                    await _caretaker.SaveMementoAsync(memento);
                }

                var uploaded = await _audioLibraryStorage.UploadAudioAsync(context);
                if (uploaded)
                {
                    await _botClient.SendMessage(memento.ChatId, $"\"{memento.FileName}\" accepted and uploaded to the library.");
                    if (File.Exists(memento.LocalFilePath)) File.Delete(memento.LocalFilePath);
                    await _caretaker.RemoveMementoAsync(memento.SessionId);
                }
                else
                {
                    _logger.LogWarning("Upload failed for session {Id}, will remain in queue.", memento.SessionId);
                    memento = memento with { Status = UploadStatus.Started };
                    await _caretaker.SaveMementoAsync(memento);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing upload session {Id}", memento.SessionId);
            memento = memento with { Status = UploadStatus.Started };
            await _caretaker.SaveMementoAsync(memento);
        }
    }

    private async Task HandleRejectionAsync(UploadSessionMemento memento, User user, ValidationResult result)
    {
        await _botClient.SendMessage(memento.ChatId, $"File rejected: {result.RejectionReason}");
        
        IViolation violation = result.RejectionReason?.Contains("spectral", StringComparison.OrdinalIgnoreCase) == true
            ? new FakeLosslessViolation()
            : new ForbiddenFormatViolation(Path.GetExtension(memento.FileName));

        _moderationMediator.ProcessViolation(user, violation);
        
        await _userService.UpdateUserAsync(user);
        
        if (user.Status != UserManagement.Entities.UserStatus.Whitelisted)
        {
            IUserState state = user.Status switch
            {
                UserManagement.Entities.UserStatus.TimedOut => new TimedOutState(user.TimeoutUntil ?? DateTime.MinValue),
                UserManagement.Entities.UserStatus.Blacklisted => new BannedState(),
                _ => new ActiveState()
            };
            await _botClient.SendMessage(memento.ChatId, $"⚠️ Warning: {state.GetStatusMessage()} Current strikes: {user.StrikeCount}");
        }

        await _caretaker.RemoveMementoAsync(memento.SessionId);
    }

    private static readonly HttpClient _httpClient = new HttpClient();

    private async Task DownloadToStreamAsync(string filePath, Stream destination)
    {
        if (!string.IsNullOrEmpty(_localPath) && !string.IsNullOrEmpty(_nginxUrl) && filePath.StartsWith(_localPath))
        {
            string downloadUrl = filePath.Replace(_localPath, _nginxUrl);
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
