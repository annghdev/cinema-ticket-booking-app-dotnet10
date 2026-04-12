namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the PricingPolicy entity.
/// Provides queries to resolve the correct pricing policy for ticket generation,
/// supporting cinema-specific overrides and fallback to default policies.
/// </summary>
public interface IPricingPolicyRepository : IRepository<PricingPolicy>
{
    /// <summary>
    /// Gets the active pricing policy for a given screen type and seat type.
    /// Prioritizes cinema-specific policy over default (CinemaId = null) policy.
    /// </summary>
    Task<PricingPolicy?> GetActivePolicyAsync(
        Guid cinemaId,
        ScreenType screenType,
        SeatType seatType,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all active pricing policies applicable for a cinema + screen type combination.
    /// Used for bulk ticket pricing during ShowTime creation.
    /// </summary>
    Task<List<PricingPolicy>> GetActivePoliciesAsync(
        Guid cinemaId,
        ScreenType screenType,
        CancellationToken ct = default);
}
