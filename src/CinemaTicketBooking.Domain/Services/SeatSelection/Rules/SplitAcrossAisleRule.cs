namespace CinemaTicketBooking.Domain;

public sealed class SplitAcrossAisleRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.SplitAcrossAisle);
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        foreach (var rowEntry in context.SelectedSeatsByRow)
        {
            var row = rowEntry.Key;
            if (!context.ActiveSeatsByRow.TryGetValue(row, out var rowSeats) || rowSeats.Count == 0)
            {
                continue;
            }

            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowSeats, aisleColumns);
            var selectedSegments = segments.Count(segment =>
                segment.Any(seat => context.SelectedSeatCodes.Contains(seat.Code)));

            if (selectedSegments <= 1)
            {
                continue;
            }

            var affectedSeats = rowEntry.Value.Select(x => x.Code).OrderBy(x => x).ToList();
            violations.Add(new SeatSelectionViolation(
                Type: SeatSelectionViolationType.SplitAcrossAisle,
                Level: level,
                Message: $"Selected seats in row {row} are split across an aisle.",
                AffectedSeats: affectedSeats));
        }

        return violations;
    }
}
