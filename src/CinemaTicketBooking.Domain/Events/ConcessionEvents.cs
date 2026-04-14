namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a concession item is created.
/// </summary>
public record ConcessionCreated(
    Guid ConcessionId,
    string Name,
    decimal Price,
    bool IsAvailable) : BaseDomainEvent;

/// <summary>
/// Raised when a concession item's basic information is updated.
/// </summary>
public record ConcessionBasicInfoUpdated(Guid ConcessionId, string Name) : BaseDomainEvent;

/// <summary>
/// Raised when a concession item is marked available for purchase.
/// </summary>
public record ConcessionMarkedAvailable(Guid ConcessionId) : BaseDomainEvent;

/// <summary>
/// Raised when a concession item is marked unavailable (e.g., out of stock).
/// </summary>
public record ConcessionMarkedUnavailable(Guid ConcessionId) : BaseDomainEvent;
