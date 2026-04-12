namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when seats are generated for a Screen from its SeatMap.
/// Side effects: admin notification, cache updates.
/// </summary>
public record ScreenSeatsGenerated(
    Guid ScreenId,
    Guid CinemaId,
    string ScreenCode,
    ScreenType ScreenType,
    int TotalSeatsGenerated) : IDomainEvent;
