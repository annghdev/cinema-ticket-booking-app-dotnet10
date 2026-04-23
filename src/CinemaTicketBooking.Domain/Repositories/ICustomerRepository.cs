namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the Customer entity.
/// Manages both registered users and guest customers identified by SessionId.
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Loads a customer by session id with change tracking. Required before linking the entity to a new
    /// booking: the default query filter uses no-tracking, which would make EF try to insert a duplicate
    /// customer row on save.
    /// </summary>
    Task<Customer?> GetTrackedBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
}
