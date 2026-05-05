namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by showtime queries.
/// </summary>
public sealed record ShowTimeDto(
    Guid Id,
    Guid MovieId,
    string MovieName,
    string MovieThumbnailUrl,
    string MovieGenre,
    int MovieDuration,
    Guid ScreenId,
    string ScreenCode,
    Guid CinemaId,
    string CinemaName,
    string CinemaAddress,
    DateOnly Date,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    ShowTimeStatus Status,
    ScreenType Format,
    int TicketCount,
    int AvailableTicketCount,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Lightweight read model for tickets in showtime detail.
/// </summary>
public sealed record ShowTimeTicketDto(
    Guid Id,
    string Code,
    decimal Price,
    TicketStatus Status,
    string? LockingBy);
