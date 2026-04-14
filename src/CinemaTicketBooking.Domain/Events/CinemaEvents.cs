namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a cinema is created.
/// </summary>
public record CinemaCreated(Guid CinemaId, string Name, string Address, bool IsActive) : BaseDomainEvent;

/// <summary>
/// Raised when basic cinema information is updated.
/// </summary>
public record CinemaBasicInfoUpdated(Guid CinemaId, string Name, string Address, bool IsActive) : BaseDomainEvent;

/// <summary>
/// Raised when a cinema is deleted (soft delete).
/// </summary>
public record CinemaDeleted(Guid CinemaId, string Name, string Address) : BaseDomainEvent;

/// <summary>
/// Raised when a cinema location becomes active again.
/// </summary>
public record CinemaActivated(Guid CinemaId, string Name, string Address) : BaseDomainEvent;

/// <summary>
/// Raised when a cinema location is deactivated.
/// </summary>
public record CinemaDeactivated(Guid CinemaId, string Name, string Address) : BaseDomainEvent;
