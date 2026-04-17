namespace CinemaTicketBooking.Application.Abstractions;

public interface ICacheService<T>
{
    Task<T?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefix(string prefix, CancellationToken ct = default);
}
