using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlac.AudioProcessing.Interfaces;
using Microsoft.Extensions.Logging;
using NurFlacUser = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class ScanCommand : AdminOnlyCommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly IAudioProcessor _audioProcessor;
    private readonly ILogger<ScanCommand> _logger;

    public ScanCommand(
        ITelegramBotClient botClient, 
        IAudioProcessor audioProcessor, 
        IConfiguration configuration,
        ILogger<ScanCommand> logger)
        : base(botClient, configuration)
    {
        _botClient = botClient;
        _audioProcessor = audioProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAdminAsync(Message message, NurFlacUser user)
    {
        var targetMessage = message.ReplyToMessage;
        if (targetMessage?.Audio == null && targetMessage?.Document == null)
        {
            await _botClient.SendMessage(message.Chat.Id, "Please reply to an audio file or document with /scan");
            return;
        }

        var fileId = targetMessage.Audio?.FileId ?? targetMessage.Document?.FileId;
        var fileName = targetMessage.Audio?.FileName ?? targetMessage.Document?.FileName ?? "unknown";

        _logger.LogInformation("[SPECTRAL] Manual scan requested by {UserId} for {FileName}", message.From?.Id, fileName);
        await _botClient.SendMessage(message.Chat.Id, $"Downloading and scanning {fileName}...");

        var file = await _botClient.GetFile(fileId!);
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(fileName));

        try
        {
            await using (var stream = File.OpenWrite(tempPath))
            {
                await _botClient.DownloadFile(file.FilePath!, stream);
            }

            var result = await _audioProcessor.AnalyzeLosslessQualityAsync(tempPath);

            _logger.LogInformation("[SPECTRAL] Manual scan result for {FileName}: {Status}. Detected: {Cutoff}Hz", 
                fileName, result.IsTrueLossless ? "SUCCESS" : "REJECTED", result.DetectedCutoffHz);

            var report = result.IsTrueLossless 
                ? $"✅ TRUE LOSSLESS\nDetected Cutoff: {result.DetectedCutoffHz:N0} Hz\nRequired: {result.RequiredCutoffHz:N0} Hz" 
                : $"❌ FAKE LOSSLESS\nDetected Cutoff: {result.DetectedCutoffHz:N0} Hz\nRequired: {result.RequiredCutoffHz:N0} Hz";

            await _botClient.SendMessage(message.Chat.Id, $"Scan Report for {fileName}:\n{report}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SPECTRAL] Manual scan failed for {FileName}", fileName);
            await _botClient.SendMessage(message.Chat.Id, $"Scan failed: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
