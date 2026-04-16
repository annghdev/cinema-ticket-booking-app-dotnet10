namespace CinemaTicketBooking.Domain;

internal static class SeatSelectionRuleHelpers
{
    /// <summary>
    /// Splits one physical row into contiguous seat segments separated by aisles.
    /// </summary>
    public static IReadOnlyList<List<Seat>> SplitByAisle(
        IReadOnlyList<Seat> rowSeats,
        IReadOnlyList<int> aisleColumns)
    {
        // 1. Normalize aisle lookup for O(1) checks when scanning seat columns.
        var aisleSet = aisleColumns.Count == 0
            ? new HashSet<int>()
            : aisleColumns.ToHashSet();
        var segments = new List<List<Seat>>();
        var current = new List<Seat>();

        // 2. Walk left-to-right and cut segment whenever an aisle exists between two seats.
        foreach (var seat in rowSeats.OrderBy(x => x.Column))
        {
            var hasAisleBetween = current.Count > 0
                                  && HasAisleBetween(current[^1].Column, seat.Column, aisleSet);
            if (hasAisleBetween)
            {
                segments.Add(current);
                current = [];
            }

            current.Add(seat);
        }

        // 3. Flush the last working segment.
        if (current.Count > 0)
        {
            segments.Add(current);
        }

        return segments;
    }

    /// <summary>
    /// Determines whether a seat should be treated as occupied by rule evaluation.
    /// </summary>
    public static bool IsOccupied(Seat seat, SeatSelectionValidationContext context)
    {
        return context.SelectedSeatCodes.Contains(seat.Code)
            || context.OccupiedSeatCodes.Contains(seat.Code);
    }

    /// <summary>
    /// Checks if at least one aisle column lies between two seat columns.
    /// </summary>
    private static bool HasAisleBetween(int leftColumn, int rightColumn, HashSet<int> aisleSet)
    {
        // Adjacent seats cannot have an aisle in between.
        if (leftColumn + 1 == rightColumn)
        {
            return false;
        }

        // Scan intermediate columns only.
        for (var column = leftColumn + 1; column < rightColumn; column++)
        {
            if (aisleSet.Contains(column))
            {
                return true;
            }
        }

        return false;
    }
}
