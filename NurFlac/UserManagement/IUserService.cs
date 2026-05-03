using NurFlac.UserManagement.Entities;

namespace NurFlac.UserManagement;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(long telegramId);
    Task ApplyStrikeAsync(long telegramId, string reason);
    Task UpdateUserAsync(User user);
}
