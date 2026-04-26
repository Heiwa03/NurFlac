using Microsoft.Extensions.DependencyInjection;
using NurFlac.Entry;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = NurFlac.UserManagement.Entities.User;

namespace NurFlac.Handlers;

public class HelpCommand : ICommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public HelpCommand(ITelegramBotClient botClient, IServiceProvider serviceProvider)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Message message, User user)
    {
        var catalog = _serviceProvider.GetRequiredService<ICommandCatalog>();

        var commands = catalog
            .GetAll()
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Name)
            .ToList();

        if (commands.Count == 0)
        {
            await _botClient.SendMessage(message.Chat.Id, "No commands are currently registered.");
            return;
        }

        var lines = new List<string>
        {
            "Available commands:"
        };

        string? currentCategory = null;
        foreach (var command in commands)
        {
            if (!string.Equals(currentCategory, command.Category, StringComparison.OrdinalIgnoreCase))
            {
                currentCategory = command.Category;
                lines.Add($"\n[{currentCategory}]");
            }

            var aliases = command.Aliases.Length > 0
                ? $" (aliases: {string.Join(", ", command.Aliases.Select(a => $"/{a}"))})"
                : string.Empty;

            lines.Add($"/{command.Name}{aliases}");
        }

        await _botClient.SendMessage(message.Chat.Id, string.Join("\n", lines));
    }
}
