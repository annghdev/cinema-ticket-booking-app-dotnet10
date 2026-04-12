namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the Booking aggregate.
/// Extends IRepository with queries that eagerly load the full Booking graph
/// (Tickets, Concessions, Customer) needed for domain operations.
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    /// <summary>
    /// Loads a Booking with all related entities (ShowTime, Tickets, Concessions, Customer).
    /// Used when domain logic needs the full aggregate — e.g., Confirm(), Cancel().
    /// Returns null if the Booking does not exist.
    /// </summary>
    Task<Booking?> LoadFullAsync(Guid id, CancellationToken ct = default);
}
