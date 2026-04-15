namespace CinemaTicketBooking.Domain;

/// <summary>
/// Represents one validation issue detected during pre-checkout seat selection.
/// </summary>
public sealed record SeatSelectionViolation(
    SeatSelectionViolationType Type,
    SeatSelectionPolicyLevel Level,
    string Message,
    IReadOnlyList<string> AffectedSeats)
{
    public bool BlockCheckout => Level == SeatSelectionPolicyLevel.Block;
}

/// <summary>
/// Carries the domain validation output consumed by pre-checkout flow.
/// </summary>
public sealed class SeatSelectionValidationResult
{
    public List<SeatSelectionViolation> Warnings { get; } = [];
    public List<SeatSelectionViolation> Errors { get; } = [];
    public List<string> Hints { get; } = [];

    public bool CanProceed => Errors.Count == 0;

    public void AddViolation(SeatSelectionViolation violation)
    {
        if (violation.Level == SeatSelectionPolicyLevel.Allow)
        {
            return;
        }

        if (violation.BlockCheckout)
        {
            Errors.Add(violation);
            return;
        }

        Warnings.Add(violation);
    }
}
