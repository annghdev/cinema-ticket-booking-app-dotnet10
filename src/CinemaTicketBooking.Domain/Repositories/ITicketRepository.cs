namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the Ticket entity.
/// Provides CRUD operations for tickets linked to ShowTimes (lifecycle: Available → Locking → Sold).
/// </summary>
public interface ITicketRepository : IRepository<Ticket>
{
}
