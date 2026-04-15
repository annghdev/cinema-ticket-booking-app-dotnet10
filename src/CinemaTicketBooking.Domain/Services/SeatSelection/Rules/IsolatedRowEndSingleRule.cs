namespace CinemaTicketBooking.Domain;

public sealed class IsolatedRowEndSingleRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.IsolatedRowEndSingle);
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        foreach (var rowEntry in context.ActiveSeatsByRow)
        {
            var row = rowEntry.Key;
            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowEntry.Value, aisleColumns);

            foreach (var segment in segments)
            {
                if (segment.Count < 2)
                {
                    continue;
                }

                var first = segment[0];
                var second = segment[1];
                if (!SeatSelectionRuleHelpers.IsOccupied(first, context)
                    && SeatSelectionRuleHelpers.IsOccupied(second, context))
                {
                    violations.Add(new SeatSelectionViolation(
                        Type: SeatSelectionViolationType.IsolatedRowEndSingle,
                        Level: level,
                        Message: $"Selection leaves an isolated single seat at row-start: {first.Code}.",
                        AffectedSeats: [first.Code]));
                }

                var last = segment[^1];
                var beforeLast = segment[^2];
                if (!SeatSelectionRuleHelpers.IsOccupied(last, context)
                    && SeatSelectionRuleHelpers.IsOccupied(beforeLast, context))
                {
                    violations.Add(new SeatSelectionViolation(
                        Type: SeatSelectionViolationType.IsolatedRowEndSingle,
                        Level: level,
                        Message: $"Selection leaves an isolated single seat at row-end: {last.Code}.",
                        AffectedSeats: [last.Code]));
                }
            }
        }

        return violations;
    }
}
