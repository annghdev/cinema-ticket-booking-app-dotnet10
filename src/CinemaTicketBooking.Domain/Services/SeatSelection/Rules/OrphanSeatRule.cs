namespace CinemaTicketBooking.Domain;

/// <summary>
/// Detects a single empty seat trapped between two occupied seats in one segment.
/// </summary>
/// <remarks>
/// Illustration (x = occupied by selection or others; . = empty seat in segment):
/// <code>
/// Orphan in the middle:
/// [x][x][.][x][x]   → center '.' is orphan (pattern x . x)
///
/// No orphan (gap is not a single seat):
/// [x][x][.][.][x]   → two adjacent empties
/// </code>
/// </remarks>
public sealed class OrphanSeatRule : ISeatSelectionRule
{
    /// <summary>
    /// Detects orphan-seat pattern: occupied-empty-occupied inside one segment.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var violations = new List<SeatSelectionViolation>();
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.OrphanSeat);
        // 1. Skip when policy allows orphan seats.
        if (level == SeatSelectionPolicyLevel.Allow)
        {
            return violations;
        }

        // 2. Evaluate each active row and each aisle-separated seat segment.
        foreach (var rowEntry in context.ActiveSeatsByRow)
        {
            var row = rowEntry.Key;
            var rowSeats = rowEntry.Value;
            var aisleColumns = context.AisleColumnsByRow.GetValueOrDefault(row, []);
            var segments = SeatSelectionRuleHelpers.SplitByAisle(rowSeats, aisleColumns);

            foreach (var segment in segments)
            {
                // 3. Sliding window over triplets to detect 1-seat orphan gaps.
                if (segment.Count >= 3)
                {
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

                // 4. Sliding window over quadruplets to detect 2-seat orphan gaps.
                if (segment.Count >= 4)
                {
                    for (var index = 1; index < segment.Count - 2; index++)
                    {
                        var leftOccupied = SeatSelectionRuleHelpers.IsOccupied(segment[index - 1], context);
                        var center1Occupied = SeatSelectionRuleHelpers.IsOccupied(segment[index], context);
                        var center2Occupied = SeatSelectionRuleHelpers.IsOccupied(segment[index + 1], context);
                        var rightOccupied = SeatSelectionRuleHelpers.IsOccupied(segment[index + 2], context);
                        if (leftOccupied && !center1Occupied && !center2Occupied && rightOccupied)
                        {
                            violations.Add(new SeatSelectionViolation(
                                Type: SeatSelectionViolationType.OrphanSeat,
                                Level: level,
                                Message: $"Selection leaves a 2-seat gap between {segment[index - 1].Code} and {segment[index + 2].Code}.",
                                AffectedSeats: [segment[index].Code, segment[index + 1].Code]));
                        }
                    }
                }
            }
        }

        return violations;
    }
}
