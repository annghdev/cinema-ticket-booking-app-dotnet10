using System.Text.Json;
using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace CinemaTicketBooking.Infrastructure.Cache;

public class RedisCacheService<T>(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer) : ICacheService<T>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync(string key, CancellationToken ct = default)
    {
        var json = await distributedCache.GetStringAsync(key, ct);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    public async Task SetAsync(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (slidingExpiration.HasValue)
        {
            options.SlidingExpiration = slidingExpiration;
        }

        var json = JsonSerializer.Serialize(value, SerializerOptions);
        await distributedCache.SetStringAsync(key, json, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return distributedCache.RemoveAsync(key, ct);
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        return RemoveKeysByPatternAsync("*", ct);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var db = connectionMultiplexer.GetDatabase();
        return await db.KeyExistsAsync(key);
    }

    public Task RemoveByPrefix(string prefix, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Task.CompletedTask;
        }

        return RemoveKeysByPatternAsync($"{prefix}*", ct);
    }

    private async Task RemoveKeysByPatternAsync(string pattern, CancellationToken ct)
    {
        var db = connectionMultiplexer.GetDatabase();
        var keys = new List<RedisKey>();

        foreach (var endpoint in connectionMultiplexer.GetEndPoints())
        {
            ct.ThrowIfCancellationRequested();
            var server = connectionMultiplexer.GetServer(endpoint);

            if (!server.IsConnected || server.IsReplica)
            {
                continue;
            }

            await foreach (var key in server.KeysAsync(db.Database, pattern: pattern))
            {
                ct.ThrowIfCancellationRequested();
                keys.Add(key);
            }
        }

        if (keys.Count == 0)
        {
            return;
        }

        await db.KeyDeleteAsync(keys.ToArray());
    }
}
