namespace CinemaTicketBooking.Application.Abstractions;

public interface ICacheService<T>
{
    Task<T?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
