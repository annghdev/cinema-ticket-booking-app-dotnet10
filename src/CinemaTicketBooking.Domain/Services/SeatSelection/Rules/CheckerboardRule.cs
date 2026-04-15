namespace CinemaTicketBooking.Domain;

public sealed class CheckerboardRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.Checkerboard);
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
                if (segment.Count < 5)
                {
                    continue;
                }

                var states = segment
                    .Select(x => SeatSelectionRuleHelpers.IsOccupied(x, context) ? '1' : '0')
                    .ToArray();
                var stateText = new string(states);
                if (!stateText.Contains("10101") && !stateText.Contains("01010"))
                {
                    continue;
                }

                violations.Add(new SeatSelectionViolation(
                    Type: SeatSelectionViolationType.Checkerboard,
                    Level: level,
                    Message: $"Selection forms a checkerboard pattern in row {row}.",
                    AffectedSeats: segment.Select(x => x.Code).OrderBy(x => x).ToList()));
            }
        }

        return violations;
    }
}
