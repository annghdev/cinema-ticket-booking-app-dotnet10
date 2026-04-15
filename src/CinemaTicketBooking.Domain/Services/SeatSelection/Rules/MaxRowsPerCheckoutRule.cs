namespace CinemaTicketBooking.Domain;

public sealed class MaxRowsPerCheckoutRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        if (context.SelectedSeatsByRow.Count <= context.Policy.MaxRowsPerCheckout)
        {
            return [];
        }

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
