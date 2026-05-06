namespace CinemaTicketBooking.Application.Features;

public sealed record SlideDto(
    Guid Id,
    string Title,
    string Description,
    string ImageUrl,
    string TargetUrl,
    int DisplayOrder,
    SlideType Type,
    string? VideoUrl
);

