namespace CinemaTicketBooking.Domain;

/// <summary>
/// Enforces <see cref="SeatSelectionPolicy.MaxRowsPerCheckout"/>.
/// </summary>
/// <remarks>
/// Illustration (x = selected; policy example MaxRows = 2):
/// <code>
/// Row A: [x][x][x]
/// Row B: [x][x][ ]
/// Row C: [x][ ][ ]   ← third row with any selection violates
///
/// Valid with MaxRows = 2:
/// Row A: [x][x][x]
/// Row B: [x][x][ ]
/// </code>
/// </remarks>
public sealed class MaxRowsPerCheckoutRule : ISeatSelectionRule
{
    /// <summary>
    /// Validates the maximum number of rows touched by current selection.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        // 1. Exit when selected rows count is within policy threshold.
        if (context.SelectedSeatsByRow.Count <= context.Policy.MaxRowsPerCheckout)
        {
            return [];
        }

        // 2. Return one violation containing all selected seats across rows.
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.MaxRows);
        return
        [
            new SeatSelectionViolation(
                Type: SeatSelectionViolationType.MaxRows,
                Level: level,
                Message: $"You can select seats across at most {context.Policy.MaxRowsPerCheckout} rows.",
                AffectedSeats: context.SelectedSeats.Select(x => x.Code).OrderBy(x => x).ToList())
        ];
    }
}
