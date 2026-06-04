using NurFlac.Users.Entities;

namespace NurFlac.Users;

public interface IUserRepository
{
    Task<User> GetOrCreateAsync(long telegramId, CancellationToken ct = default);
    Task       UpdateAsync(User user,            CancellationToken ct = default);
    Task<int>  ClearAllAsync(CancellationToken ct = default);
    Task       ResetAsync(long telegramId,       CancellationToken ct = default);
}
