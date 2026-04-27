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
        await _botClient.SendMessage(message.Chat.Id, "Running storage diagnostics...");

        var canConnect = await _storageService.CheckConnectionAsync();
        if (!canConnect)
        {
            await _botClient.SendMessage(message.Chat.Id, "Could not connect to storage backend.");
            return;
        }

        await _botClient.SendMessage(message.Chat.Id, "Storage connection OK.");

        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "dummy content for testing");

        try
        {
            var uploaded = await _storageService.UploadFileAsync(tempFile, "test-upload.txt", string.Empty);

            if (uploaded)
                await _botClient.SendMessage(message.Chat.Id, "Test file uploaded successfully.");
            else
                await _botClient.SendMessage(message.Chat.Id, "Upload failed.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}