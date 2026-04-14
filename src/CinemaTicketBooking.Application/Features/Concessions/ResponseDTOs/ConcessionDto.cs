namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by concession queries.
/// </summary>
public sealed record ConcessionDto(
    Guid Id,
    string Name,
    decimal Price,
    string ImageUrl,
    bool IsAvailable,
    DateTimeOffset CreatedAt
);
