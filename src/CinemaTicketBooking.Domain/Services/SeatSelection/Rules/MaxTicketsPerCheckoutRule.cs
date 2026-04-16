namespace CinemaTicketBooking.Domain;

/// <summary>
/// Enforces <see cref="SeatSelectionPolicy.MaxTicketsPerCheckout"/>.
/// </summary>
/// <remarks>
/// Illustration (x = seat selected in this checkout; policy example MaxTickets = 8):
/// <code>
/// One row, 9 selections:
/// [x][x][x][x][x][x][x][x][x]  → violates (count &gt; 8)
///
/// Valid with MaxTickets = 8:
/// [x][x][x][x][x][x][x][x][ ]  → 8 seats only
/// </code>
/// </remarks>
public sealed class MaxTicketsPerCheckoutRule : ISeatSelectionRule
{
    /// <summary>
    /// Validates the maximum number of selected tickets in one checkout.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        // 1. Exit when current selection is inside the configured limit.
        if (context.SelectedTickets.Count <= context.Policy.MaxTicketsPerCheckout)
        {
            return [];
        }

        // 2. Return one violation carrying all selected seats for UI highlighting.
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.MaxTickets);
        return
        [
            new SeatSelectionViolation(
                Type: SeatSelectionViolationType.MaxTickets,
                Level: level,
                Message: $"You can select up to {context.Policy.MaxTicketsPerCheckout} tickets per checkout.",
                AffectedSeats: context.SelectedSeats.Select(x => x.Code).OrderBy(x => x).ToList())
        ];
    }
}
