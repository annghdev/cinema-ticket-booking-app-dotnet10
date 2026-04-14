namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a movie is created.
/// </summary>
public record MovieCreated(
    Guid MovieId,
    string MovieName,
    MovieStatus Status,
    MovieGenre Genre) : BaseDomainEvent;

/// <summary>
/// Raised when basic movie metadata is updated.
/// </summary>
public record MovieBasicInfoUpdated(
    Guid MovieId,
    string MovieName,
    MovieStatus Status,
    MovieGenre Genre) : BaseDomainEvent;

/// <summary>
/// Raised when a movie is deleted (soft delete).
/// </summary>
public record MovieDeleted(Guid MovieId, string MovieName) : BaseDomainEvent;

/// <summary>
/// Raised when a movie moves from Ongoing to NowShowing (eligible for ShowTime scheduling).
/// </summary>
public record MoviePromotedToNowShowing(Guid MovieId, string MovieName) : BaseDomainEvent;

/// <summary>
/// Raised when an upcoming movie (Ongoing) is withdrawn before it reaches screens.
/// </summary>
public record MovieWithdrawnAsNoShowWhileUpcoming(Guid MovieId, string MovieName) : BaseDomainEvent;

/// <summary>
/// Raised when a movie that is currently on screens (NowShowing) is closed and marked NoShow.
/// </summary>
public record MovieRunClosedAsNoShow(Guid MovieId, string MovieName) : BaseDomainEvent;
