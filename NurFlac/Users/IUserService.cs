using NurFlac.Users.Entities;

namespace NurFlac.Users;

public interface IUserService
{
    Task<User> GetOrCreateAsync(long telegramId,                       CancellationToken ct = default);
    Task       ApplyStrikeAsync(long telegramId, int score,            CancellationToken ct = default);
    Task       TimeoutAsync(long telegramId, TimeSpan duration,        CancellationToken ct = default);
    Task       BanAsync(long telegramId,                               CancellationToken ct = default);
    Task       UnbanAsync(long telegramId,                             CancellationToken ct = default);
    Task       ResetUserAsync(long telegramId,                         CancellationToken ct = default);
    Task<int>  ClearAllUsersAsync(                                     CancellationToken ct = default);
}
