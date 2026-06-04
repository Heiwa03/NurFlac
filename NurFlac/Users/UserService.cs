using NurFlac.Users.Entities;

namespace NurFlac.Users;

public sealed class UserService(IUserRepository repository) : IUserService
{
    public Task<User> GetOrCreateAsync(long telegramId, CancellationToken ct = default)
        => repository.GetOrCreateAsync(telegramId, ct);

    public async Task ApplyStrikeAsync(long telegramId, int score, CancellationToken ct = default)
    {
        var user = await repository.GetOrCreateAsync(telegramId, ct);
        user.StrikeCount += score;

        // Strike thresholds:
        //   3 → 24-hour timeout
        //   4 → 72-hour timeout
        //   5+ → permanent ban
        if (user.StrikeCount >= 5)
        {
            user.Status       = UserStatus.Banned;
            user.TimeoutUntil = null;
        }
        else if (user.StrikeCount == 4)
        {
            user.Status       = UserStatus.TimedOut;
            user.TimeoutUntil = DateTime.UtcNow.AddHours(72);
        }
        else if (user.StrikeCount == 3)
        {
            user.Status       = UserStatus.TimedOut;
            user.TimeoutUntil = DateTime.UtcNow.AddHours(24);
        }

        await repository.UpdateAsync(user, ct);
    }

    public async Task TimeoutAsync(long telegramId, TimeSpan duration, CancellationToken ct = default)
    {
        var user = await repository.GetOrCreateAsync(telegramId, ct);
        user.Status       = UserStatus.TimedOut;
        user.TimeoutUntil = DateTime.UtcNow.Add(duration);
        await repository.UpdateAsync(user, ct);
    }

    public async Task BanAsync(long telegramId, CancellationToken ct = default)
    {
        var user = await repository.GetOrCreateAsync(telegramId, ct);
        user.Status       = UserStatus.Banned;
        user.TimeoutUntil = null;
        await repository.UpdateAsync(user, ct);
    }

    public async Task UnbanAsync(long telegramId, CancellationToken ct = default)
    {
        var user = await repository.GetOrCreateAsync(telegramId, ct);
        user.Status       = UserStatus.Active;
        user.TimeoutUntil = null;
        await repository.UpdateAsync(user, ct);
    }

    public Task ResetUserAsync(long telegramId, CancellationToken ct = default)
        => repository.ResetAsync(telegramId, ct);

    public Task<int> ClearAllUsersAsync(CancellationToken ct = default)
        => repository.ClearAllAsync(ct);
}
