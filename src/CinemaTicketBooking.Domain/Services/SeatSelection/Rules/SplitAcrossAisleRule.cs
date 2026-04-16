namespace CinemaTicketBooking.Domain;

/// <summary>
/// Detects split selection across an aisle within the same physical row.
/// </summary>
/// <remarks>
/// Illustration (| = aisle / stair column 0 in seat map; x = selected in same row):
/// <code>
/// Same row, two segments both touched:
/// [x][x] | [x][ ]   → violates (selection spans both sides of aisle)
///
/// Valid (one segment only):
/// [x][x] | [ ][ ]   → all picks on left segment
/// </code>
/// </remarks>
public sealed class SplitAcrossAisleRule : ISeatSelectionRule
{
    /// <summary>
    /// Detects selections that span multiple aisle-separated segments in the same row.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.SplitAcrossAisle);
        // 1. Skip evaluation when policy explicitly allows this pattern.
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        // 2. Evaluate each row where user has selected at least one seat.
        foreach (var rowEntry in context.SelectedSeatsByRow)
        {
            var row = rowEntry.Key;
            if (!context.ActiveSeatsByRow.TryGetValue(row, out var rowSeats) || rowSeats.Count == 0)
            {
                continue;
            }

            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowSeats, aisleColumns);
            // 3. Count how many aisle-separated segments contain selected seats.
            var selectedSegments = segments.Count(segment =>
                segment.Any(seat => context.SelectedSeatCodes.Contains(seat.Code)));

            if (selectedSegments <= 1)
            {
                continue;
            }

            // 4. A violation is created when one row spans more than one segment.
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
