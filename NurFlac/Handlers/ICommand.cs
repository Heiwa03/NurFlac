using Telegram.Bot.Types;
using NurFlac.UserManagement.Entities;

namespace NurFlac.Handlers;

public interface ICommand
{
    Task ExecuteAsync(Message message, UserManagement.Entities.User user);
}
