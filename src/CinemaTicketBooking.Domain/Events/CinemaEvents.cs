namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a cinema's active status changes (activated or deactivated).
/// Side effects: update listing cache, notify affected screen schedules.
/// </summary>
public record CinemaStatusChanged(
    Guid CinemaId,
    string Name,
    string Address,
    bool IsActive) : IDomainEvent;
