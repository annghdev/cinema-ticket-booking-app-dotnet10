namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the Ticket entity.
/// Provides CRUD operations for tickets linked to ShowTimes
/// (lifecycle: Available → Locking → PendingPayment → Sold).
/// </summary>
public interface ITicketRepository : IRepository<Ticket>
{
    /// <summary>
    /// Attempts to acquire a transaction-scoped database guard for ticket locking.
    /// </summary>
    Task<bool> TryAcquireLockGuardAsync(Guid ticketId, CancellationToken ct = default);
}
