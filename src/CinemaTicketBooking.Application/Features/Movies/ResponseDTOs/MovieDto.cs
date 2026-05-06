namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by movie queries.
/// </summary>
public sealed record MovieDto(
    Guid Id,
    string Name,
    string Description,
    string ThumbnailUrl,
    string Studio,
    string Director,
    string? OfficialTrailerUrl,
    int Duration,
    MovieGenre Genre,
    MovieStatus Status,
    decimal TargetReach,
    DateTimeOffset CreatedAt
);
