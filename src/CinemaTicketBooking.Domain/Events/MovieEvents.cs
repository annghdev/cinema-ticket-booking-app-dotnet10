namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a movie's status changes (e.g., Ongoing → NowShowing, NowShowing → NoShow).
/// Side effects: invalidate schedule cache, notify if movie becomes unavailable.
/// </summary>
public record MovieStatusChanged(
    Guid MovieId,
    string MovieName,
    MovieStatus OldStatus,
    MovieStatus NewStatus) : IDomainEvent;
