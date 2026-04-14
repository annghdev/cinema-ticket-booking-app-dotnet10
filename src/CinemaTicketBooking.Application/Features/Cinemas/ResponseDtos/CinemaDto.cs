namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by cinema queries.
/// </summary>
public sealed record CinemaDto(
    Guid Id,
    string Name,
    string ThumbnailUrl,
    string? Geo,
    string Address,
    bool IsActive,
    DateTimeOffset CreatedAt
);
