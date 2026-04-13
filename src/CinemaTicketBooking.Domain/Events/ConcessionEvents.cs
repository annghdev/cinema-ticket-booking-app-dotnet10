namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a concession item is marked available for purchase.
/// </summary>
public record ConcessionMarkedAvailable(Guid ConcessionId, string Name, decimal Price) : IDomainEvent;

/// <summary>
/// Raised when a concession item is marked unavailable (e.g., out of stock).
/// </summary>
public record ConcessionMarkedUnavailable(Guid ConcessionId, string Name, decimal Price) : IDomainEvent;
