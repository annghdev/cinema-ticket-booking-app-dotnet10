namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by get-showtime-by-id queries.
/// </summary>
public sealed record ShowTimeDetailDto(
    Guid Id,
    Guid MovieId,
    string MovieName,
    Guid ScreenId,
    string ScreenCode,
    Guid CinemaId,
    string CinemaName,
    DateOnly Date,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    ShowTimeStatus Status,
    int TicketCount,
    int AvailableTicketCount,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ShowTimeTicketDto> Tickets
);
