namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Lightweight DTO for concession dropdowns.
/// </summary>
public sealed record ConcessionDropdownDto(
    Guid Id,
    string Name
);
