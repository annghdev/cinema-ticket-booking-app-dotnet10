namespace CinemaTicketBooking.Domain;

/// <summary>
/// Detects alternating occupied / empty pattern that traps remaining seats.
/// </summary>
/// <remarks>
/// Illustration (x = occupied; . = empty; substring 10101 or 01010 in one segment triggers):
/// <code>
/// Checkerboard-style (5-seat window):
/// [x][.][x][.][x]   → bit pattern 10101
/// [.][x][.][x][.]   → bit pattern 01010
///
/// The rule scans each segment's occupancy string for those substrings.
/// </code>
/// </remarks>
public sealed class CheckerboardRule : ISeatSelectionRule
{
    /// <summary>
    /// Detects alternating occupancy pattern (checkerboard) inside one segment.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.Checkerboard);
        // 1. Skip when policy allows checkerboard patterns.
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        // 2. Evaluate each row/segment independently (aisle separates patterns).
        foreach (var rowEntry in context.ActiveSeatsByRow)
        {
            var row = rowEntry.Key;
            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowEntry.Value, aisleColumns);

            foreach (var segment in segments)
            {
                // Minimum length 5 for canonical alternating pattern checks.
                if (segment.Count < 5)
                {
                    continue;
                }

                // 3. Convert occupancy states to bit string and match known patterns.
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
