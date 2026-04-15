namespace CinemaTicketBooking.Domain;

internal static class SeatSelectionRuleHelpers
{
    public static IReadOnlyList<List<Seat>> SplitByAisle(
        IReadOnlyList<Seat> rowSeats,
        IReadOnlyList<int> aisleColumns)
    {
        var aisleSet = aisleColumns.Count == 0
            ? new HashSet<int>()
            : aisleColumns.ToHashSet();
        var segments = new List<List<Seat>>();
        var current = new List<Seat>();

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

        if (current.Count > 0)
        {
            segments.Add(current);
        }

        return segments;
    }

    public static bool IsOccupied(Seat seat, SeatSelectionValidationContext context)
    {
        return context.SelectedSeatCodes.Contains(seat.Code)
            || context.OccupiedSeatCodes.Contains(seat.Code);
    }

    private static bool HasAisleBetween(int leftColumn, int rightColumn, HashSet<int> aisleSet)
    {
        if (leftColumn + 1 == rightColumn)
        {
            return false;
        }

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
