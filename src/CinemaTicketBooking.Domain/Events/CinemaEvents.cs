namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a cinema location becomes active again.
/// </summary>
public record CinemaActivated(Guid CinemaId, string Name, string Address) : IDomainEvent;

/// <summary>
/// Raised when a cinema location is deactivated.
/// </summary>
public record CinemaDeactivated(Guid CinemaId, string Name, string Address) : IDomainEvent;
