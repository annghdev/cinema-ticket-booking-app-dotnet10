namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Lightweight DTO for showtime dropdowns.
/// </summary>
public sealed record ShowTimeDropdownDto(
    Guid Id,
    string DisplayName
);
