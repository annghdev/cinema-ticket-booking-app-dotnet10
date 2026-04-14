namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Coordinates ticket lock acquisition with infrastructure-specific fallback.
/// </summary>
public interface ITicketLocker
{
    /// <summary>
    /// Attempts to acquire a ticket lock.
    /// </summary>
    Task<bool> TryLockAsync(
        Guid ticketId,
        string lockOwner,
        TimeSpan ttl,
        CancellationToken ct = default);

    /// <summary>
    /// Releases a transient lock key when lock ownership matches.
    /// </summary>
    Task ReleaseAsync(Guid ticketId, string lockOwner, CancellationToken ct = default);
}
