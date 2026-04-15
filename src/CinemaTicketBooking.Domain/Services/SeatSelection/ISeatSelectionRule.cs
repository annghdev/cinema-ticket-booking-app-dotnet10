namespace CinemaTicketBooking.Domain;

/// <summary>
/// Strategy contract for a single seat-selection validation rule.
/// </summary>
public interface ISeatSelectionRule
{
    IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context);
}
