using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlac.Storage;
using NurFlac.Validation;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class SingleAudioUploadCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly AudioLibraryStorage _audioLibraryStorage;
    private readonly ILosslessAudioValidator _validator;
    private readonly ILogger<SingleAudioUploadCommand> _logger;
    private readonly string _botToken;

    public SingleAudioUploadCommand(
        ITelegramBotClient botClient,
        AudioLibraryStorage audioLibraryStorage,
        ILosslessAudioValidator validator,
        ILogger<SingleAudioUploadCommand> logger,
        IConfiguration configuration)
    {
        _botClient = botClient;
        _audioLibraryStorage = audioLibraryStorage;
        _validator = validator;
        _logger = logger;
        _botToken = configuration["TelegramBot:Token"] ?? string.Empty;
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

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) && mimeType is not null)
            extension = MimeToExtension(mimeType);

        var context = new AudioFileContext(fileName, extension, mimeType, fileId);

        // Step 1 + Step 2: extension and MIME check — no download required
        var preCheck = await _validator.ValidateAsync(context);
        if (!preCheck.IsValid)
        {
            await _botClient.SendMessage(message.Chat.Id, $"File rejected: {preCheck.RejectionReason}");
            return;
        }

        await _botClient.SendMessage(message.Chat.Id, "Format check passed. Downloading for analysis...");

        var tempPath = Path.Combine(Path.GetTempPath(), $"nurflac_{Guid.NewGuid():N}{extension}");
        try
        {
            var tgFile = await _botClient.GetFile(fileId);
            await using (var fs = File.OpenWrite(tempPath))
                await DownloadToStreamAsync(tgFile.FilePath!, fs);

            // Set local path so Step 3 (spectral) can execute
            context.LocalFilePath = tempPath;

            var fullCheck = await _validator.ValidateAsync(context);
            if (!fullCheck.IsValid)
            {
                await _botClient.SendMessage(message.Chat.Id, $"File rejected: {fullCheck.RejectionReason}");
                return;
            }

            var uploaded = await _audioLibraryStorage.UploadAudioAsync(context);
            if (uploaded)
                await _botClient.SendMessage(message.Chat.Id, $"\"{fileName}\" accepted and uploaded to the library.");
            else
                await _botClient.SendMessage(message.Chat.Id, "Upload to storage failed. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio upload failed for {FileName}", fileName);
            await _botClient.SendMessage(message.Chat.Id, "An error occurred while processing your file.");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // Add this at the class level if you aren't injecting HttpClient via DI
    private static readonly HttpClient _httpClient = new HttpClient();

    private async Task DownloadToStreamAsync(string filePath, Stream destination)
    {
        // 1. Check if the filePath is an absolute path from the local Telegram server
        if (filePath.StartsWith("/var/lib/telegram-bot-api/"))
        {
            // 2. Define the path we want to strip and the URL we want to replace it with
            string localBasePath = "/var/lib/telegram-bot-api/";
            string nginxBaseUrl = "http://100.91.112.145:8082/";

            // 3. Strip the path and create the final download URL
            string downloadUrl = filePath.Replace(localBasePath, nginxBaseUrl);

            _logger.LogInformation("Local mode Nginx download: {Url}", downloadUrl);

            // 4. Download directly from Nginx, bypassing the Telegram library entirely
            using var responseStream = await _httpClient.GetStreamAsync(downloadUrl);
            await responseStream.CopyToAsync(destination);
            return;
        }

        // Fallback: If it's a standard relative path (non-local mode), use the bot client
        await _botClient.DownloadFile(filePath, destination);
    }
    /*
    // In local server mode (TELEGRAM_LOCAL=1), getFile returns an absolute path like:
    //   /var/lib/telegram-bot-api/{token}/{relative_path}
    // DownloadFile builds: {baseUrl}/file/bot{token}/{relative_path} — so we just need
    // to strip the leading work-dir+token prefix to recover the relative path.
    private async Task DownloadToStreamAsync(string filePath, Stream destination)
    {
        if (filePath.StartsWith('/') && !string.IsNullOrEmpty(_botToken))
        {
            var marker = $"/{_botToken}/";
            var idx = filePath.IndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var relativePath = filePath[(idx + marker.Length)..];
                _logger.LogInformation("Local mode download: relative path = {RelativePath}", relativePath);
                await _botClient.DownloadFile(relativePath, destination);
                return;
            }
        }
        await _botClient.DownloadFile(filePath, destination);
    }*/

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
