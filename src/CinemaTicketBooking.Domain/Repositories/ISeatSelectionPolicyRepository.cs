namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for global seat-selection validation policies.
/// </summary>
public interface ISeatSelectionPolicyRepository : IRepository<SeatSelectionPolicy>
{
    /// <summary>
    /// Returns the active global-default policy when available.
    /// </summary>
    Task<SeatSelectionPolicy?> GetActiveGlobalAsync(CancellationToken ct = default);
}
