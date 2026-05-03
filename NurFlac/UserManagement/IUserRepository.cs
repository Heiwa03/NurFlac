using NurFlac.UserManagement.Entities;

namespace NurFlac.UserManagement;

public interface IUserRepository
{
    Task<User> GetOrCreateByTelegramIdAsync(long telegramId);
    Task UpdateUserAsync(User user);
}
