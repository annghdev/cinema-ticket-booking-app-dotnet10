namespace CinemaTicketBooking.Domain;

public interface IShowTimeRepository : IRepository<ShowTime>
{
    Task<Booking?> LoadFullAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all non-cancelled ShowTimes for a given Screen within a date range.
    /// Used by ShowTimeSchedulingService for conflict detection.
    /// </summary>
    Task<List<ShowTime>> GetActiveByScreenAndDateRangeAsync(
        Guid screenId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default);
}
