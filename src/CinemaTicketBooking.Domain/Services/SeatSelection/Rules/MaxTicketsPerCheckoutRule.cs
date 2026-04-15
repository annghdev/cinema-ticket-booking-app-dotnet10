namespace CinemaTicketBooking.Domain;

public sealed class MaxTicketsPerCheckoutRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        if (context.SelectedTickets.Count <= context.Policy.MaxTicketsPerCheckout)
        {
            return [];
        }

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
