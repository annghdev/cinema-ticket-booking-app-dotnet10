namespace CinemaTicketBooking.Domain;

public sealed class OrphanSeatRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.OrphanSeat);
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        foreach (var rowEntry in context.ActiveSeatsByRow)
        {
            var row = rowEntry.Key;
            var rowSeats = rowEntry.Value;
            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowSeats, aisleColumns);

            foreach (var segment in segments)
            {
                if (segment.Count < 3)
                {
                    continue;
                }

                for (var index = 1; index < segment.Count - 1; index++)
                {
                    var leftOccupied = SeatSelectionRuleHelpers.IsOccupied(segment[index - 1], context);
                    var centerOccupied = SeatSelectionRuleHelpers.IsOccupied(segment[index], context);
                    var rightOccupied = SeatSelectionRuleHelpers.IsOccupied(segment[index + 1], context);
                    if (leftOccupied && !centerOccupied && rightOccupied)
                    {
                        violations.Add(new SeatSelectionViolation(
                            Type: SeatSelectionViolationType.OrphanSeat,
                            Level: level,
                            Message: $"Selection leaves an orphan seat at {segment[index].Code}.",
                            AffectedSeats: [segment[index].Code]));
                    }
                }
            }
        }

        return violations;
    }
}
