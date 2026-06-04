// ============================================================
// PATTERN: Proxy (Structural)
// Role   : Proxy — intercepts IUserRepository calls and caches
//          user records in memory (short TTL for ban/timeout
//          status) to avoid a DB round-trip on every message.
//          The underlying SqliteUserRepository is the RealSubject.
// ============================================================
using Microsoft.Extensions.Caching.Memory;
using NurFlac.Users.Entities;

namespace NurFlac.Users;

public sealed class CachingUserRepositoryProxy(
    IUserRepository   inner,
    IMemoryCache      cache) : IUserRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private static string CacheKey(long id) => $"user:{id}";

    public async Task<User> GetOrCreateAsync(long telegramId, CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey(telegramId), out User? cached) && cached is not null)
            return cached;

        var user = await inner.GetOrCreateAsync(telegramId, ct);
        cache.Set(CacheKey(telegramId), user, CacheTtl);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await inner.UpdateAsync(user, ct);
        // Invalidate so the next read fetches fresh state.
        cache.Remove(CacheKey(user.TelegramId));
    }

    public async Task ResetAsync(long telegramId, CancellationToken ct = default)
    {
        await inner.ResetAsync(telegramId, ct);
        cache.Remove(CacheKey(telegramId));
    }

    public Task<int> ClearAllAsync(CancellationToken ct = default)
        // No per-key invalidation needed here — cached entries expire within 30s.
        // The underlying repository wipes the table; stale cache entries won't be promoted back.
        => inner.ClearAllAsync(ct);
}
