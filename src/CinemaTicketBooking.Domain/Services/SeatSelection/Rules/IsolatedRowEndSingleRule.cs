namespace CinemaTicketBooking.Domain;

/// <summary>
/// Detects a lone empty seat at the start or end of an aisle-bounded segment next to occupied seats.
/// </summary>
/// <remarks>
/// Illustration (x = occupied; . = empty at segment edge):
/// <code>
/// Isolated at segment start:
/// [.][x][x][x][x][x][ ][ ]   → first '.' is isolated single at row-start of segment
///
/// Isolated at segment end:
/// [ ][x][x][x][x][x][.]      → last '.' is isolated single at row-end of segment
///
/// Not isolated (edge has neighbor empty):
/// [.][.][x][x]   → start has pair of empties
/// </code>
/// </remarks>
public sealed class IsolatedRowEndSingleRule : ISeatSelectionRule
{
    /// <summary>
    /// Detects isolated single empty seats at segment boundaries (row start/end).
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.IsolatedRowEndSingle);
        // 1. Skip when policy allows row-end isolated single seats.
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        // 2. Evaluate each aisle-separated segment to avoid cross-aisle false positives.
        foreach (var rowEntry in context.ActiveSeatsByRow)
        {
            var row = rowEntry.Key;
            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowEntry.Value, aisleColumns);

            foreach (var segment in segments)
            {
                // Need at least two seats to evaluate start/end edge pair.
                if (segment.Count < 2)
                {
                    continue;
                }

                // 3. Start-edge check: empty first + occupied second.
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

                // 4. End-edge check: occupied before last + empty last.
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
