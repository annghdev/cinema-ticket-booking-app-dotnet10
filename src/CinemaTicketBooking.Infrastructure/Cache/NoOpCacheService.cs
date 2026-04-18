using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Infrastructure.Cache;

/// <summary>
/// No Redis: cache interface is inert so callers can still resolve <see cref="ICacheService"/>.
/// </summary>
public sealed class NoOpCacheService : ICacheService
{
    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult<T?>(default);

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) => Task.FromResult(false);

    /// <inheritdoc />
    public Task RemoveByPrefix(string prefix, CancellationToken ct = default) => Task.CompletedTask;
}
