using System.Collections.Concurrent;
using System.Text.Json;
using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fakes;

/// <summary>
/// In-memory <see cref="ICacheService"/> for tests (JSON round-trip matches Redis cache behavior).
/// </summary>
public sealed class InMemoryCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, string> _jsonByKey = new();

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!_jsonByKey.TryGetValue(key, out var json) || string.IsNullOrWhiteSpace(json))
            return Task.FromResult<T?>(default);

        return Task.FromResult(JsonSerializer.Deserialize<T>(json, SerializerOptions));
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        _jsonByKey[key] = json;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _jsonByKey.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken ct = default)
    {
        _jsonByKey.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(_jsonByKey.ContainsKey(key));

    /// <inheritdoc />
    public Task RemoveByPrefix(string prefix, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return Task.CompletedTask;

        foreach (var key in _jsonByKey.Keys.ToArray())
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
                _jsonByKey.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
