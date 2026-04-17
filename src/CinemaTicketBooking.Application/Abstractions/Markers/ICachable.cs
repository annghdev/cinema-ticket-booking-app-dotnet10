namespace CinemaTicketBooking.Application.Abstractions;

public interface ICachable
{
    string CacheKey { get; }
    TimeSpan? SlidingExpiration { get; }
}
