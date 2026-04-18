namespace CinemaTicketBooking.Application.Abstractions;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default);

    Task RemoveAsync(string key, CancellationToken ct = default);

    Task ClearAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    Task RemoveByPrefix(string prefix, CancellationToken ct = default);
}
