namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a concession item's availability changes.
/// Side effects: update real-time menu display, notify staff.
/// </summary>
public record ConcessionAvailabilityChanged(
    Guid ConcessionId,
    string Name,
    bool IsAvailable,
    decimal Price) : IDomainEvent;
