namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Lightweight DTO for pricing policy dropdowns.
/// </summary>
public sealed record PricingPolicyDropdownDto(
    Guid Id,
    string Name
);
