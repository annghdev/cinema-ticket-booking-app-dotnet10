namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a movie moves from Ongoing to NowShowing (eligible for ShowTime scheduling).
/// </summary>
public record MoviePromotedToNowShowing(Guid MovieId, string MovieName) : IDomainEvent;

/// <summary>
/// Raised when an upcoming movie (Ongoing) is withdrawn before it reaches screens.
/// </summary>
public record MovieWithdrawnAsNoShowWhileUpcoming(Guid MovieId, string MovieName) : IDomainEvent;

/// <summary>
/// Raised when a movie that is currently on screens (NowShowing) is closed and marked NoShow.
/// </summary>
public record MovieRunClosedAsNoShow(Guid MovieId, string MovieName) : IDomainEvent;
