using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using NurFlac.Storage;
using NurFlac.DuplicateChecking;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class TestUploadCommand : AdminOnlyCommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly IStorageService _storageService;
    private readonly IDuplicateCheckFacade _duplicateCheckFacade;

    public TestUploadCommand(
        ITelegramBotClient botClient,
        IStorageService storageService,
        IDuplicateCheckFacade duplicateCheckFacade,
        IConfiguration configuration)
        : base(botClient, configuration)
    {
        _botClient = botClient;
        _storageService = storageService;
        _duplicateCheckFacade = duplicateCheckFacade;
    }

    protected override async Task ExecuteAdminAsync(Message message, User user)
    {
        await _botClient.SendMessage(message.Chat.Id, "Testing WebDAV connection...");

        var connected = await _storageService.CheckConnectionAsync();
        if (!connected)
        {
            await _botClient.SendMessage(message.Chat.Id, "WebDAV connection failed.");
            return;
        }

        // Create a small test file
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, $"NurFlac upload test at {DateTimeOffset.Now}");

        try
        {
            var duplicateCheck = await _duplicateCheckFacade.CheckAsync(tempFile);
            if (duplicateCheck.IsDuplicate)
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Duplicate detected before upload. Fingerprint source: {duplicateCheck.Fingerprint.ProviderName}");
                return;
            }

            var uploaded = await _storageService.UploadFileAsync(tempFile, "nurflac-test.txt", "");

            if (uploaded)
            {
                await _duplicateCheckFacade.RegisterUploadedAsync(duplicateCheck, "nurflac-test.txt", user.TelegramId);
            }

            await _botClient.SendMessage(message.Chat.Id,
                uploaded ? "Upload successful! Check your WebDAV server for 'nurflac-test.txt'."
                         : "Upload failed. Check server logs.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}