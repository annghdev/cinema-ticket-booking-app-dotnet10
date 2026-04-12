namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a pricing policy is updated (price or coefficient change).
/// Side effects: invalidate pricing cache, recalculate affected future showtimes.
/// </summary>
public record PricingPolicyUpdated(
    Guid PricingPolicyId,
    Guid? CinemaId,
    ScreenType ScreenType,
    SeatType SeatType,
    decimal OldBasePrice,
    decimal NewBasePrice,
    decimal OldScreenCoefficient,
    decimal NewScreenCoefficient) : IDomainEvent;
