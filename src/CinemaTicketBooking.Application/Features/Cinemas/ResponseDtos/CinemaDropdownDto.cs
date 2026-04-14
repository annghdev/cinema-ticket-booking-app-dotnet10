namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Lightweight DTO for cinema dropdowns.
/// </summary>
public sealed record CinemaDropdownDto(
    Guid Id,
    string Name
);
