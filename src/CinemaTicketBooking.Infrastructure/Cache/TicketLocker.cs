using CinemaTicketBooking.Application.Abstractions;
using StackExchange.Redis;

namespace CinemaTicketBooking.Infrastructure.Cache;

/// <summary>
/// Redis-backed transient lock gate for ticket locking.
/// </summary>
public class RedisTicketLocker(IConnectionMultiplexer redis, ITicketRepository tickets) : ITicketLocker
{
    private const string SafeReleaseScript =
        """
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            return redis.call('DEL', KEYS[1])
        end
        return 0
        """;

    public async Task<bool> TryLockAsync(
        Guid ticketId,
        string lockOwner,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        try
        {
            var key = BuildKey(ticketId);
            var ok = await redis.GetDatabase().StringSetAsync(
                key,
                lockOwner,
                ttl,
                when: When.NotExists);

            return ok;
        }
        catch (RedisException)
        {
            var hasDbGuard = await tickets.TryAcquireLockGuardAsync(ticketId, ct);
            return hasDbGuard;
        }
    }

    public async Task ReleaseAsync(Guid ticketId, string lockOwner, CancellationToken ct = default)
    {
        try
        {
            var key = BuildKey(ticketId);
            await redis.GetDatabase().ScriptEvaluateAsync(
                SafeReleaseScript,
                [new RedisKey(key)],
                [new RedisValue(lockOwner)]);
        }
        catch (RedisException)
        {
            // Best-effort cleanup only. Database state is source of truth.
        }
    }

    private static string BuildKey(Guid ticketId)
    {
        return $"ticket:lock:{ticketId:N}";
    }
}

/// <summary>
/// Degraded lock gate when Redis is unavailable or not configured.
/// </summary>
public class NoRedisTicketLocker : ITicketLocker
{
    private readonly ITicketRepository _tickets;

    public NoRedisTicketLocker(ITicketRepository tickets)
    {
        _tickets = tickets;
    }

    public Task<bool> TryLockAsync(
        Guid ticketId,
        string lockOwner,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        return AcquireFromDatabaseAsync(ticketId, ct);
    }

    public Task ReleaseAsync(Guid ticketId, string lockOwner, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    private async Task<bool> AcquireFromDatabaseAsync(Guid ticketId, CancellationToken ct)
    {
        return await _tickets.TryAcquireLockGuardAsync(ticketId, ct);
    }
}
