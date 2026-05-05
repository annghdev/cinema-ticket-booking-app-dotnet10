namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Read model used by pricing policy queries.
/// </summary>
public sealed record PricingPolicyDto(
    Guid Id,
    Guid? CinemaId,
    ScreenType ScreenType,
    SeatType SeatType,
    decimal BasePrice,
    decimal ScreenCoefficient,
    decimal WeekendCoefficient,
    decimal FinalPrice,
    bool IsActive,
    DateTimeOffset CreatedAt
);
