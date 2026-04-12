namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the ShowTime aggregate.
/// Extends IRepository with scheduling queries (conflict detection)
/// and full aggregate loading for domain operations.
/// </summary>
public interface IShowTimeRepository : IRepository<ShowTime>
{
    /// <summary>
    /// Loads a ShowTime with all related entities (Movie, Screen, Tickets).
    /// Returns null if the ShowTime does not exist.
    /// </summary>
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
