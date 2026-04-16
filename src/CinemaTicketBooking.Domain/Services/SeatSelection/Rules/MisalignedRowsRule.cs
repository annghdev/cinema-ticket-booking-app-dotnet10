namespace CinemaTicketBooking.Domain;

/// <summary>
/// Requires multi-row selections to share the same ordered column set (rectangular block).
/// </summary>
/// <remarks>
/// Illustration (x = selected; rows must have identical column indices when sorted):
/// <code>
/// Violates (different column sets / offsets):
/// Row A: [x][x][ ][ ]
/// Row B: [ ][x][x][ ]
///
/// Violates (same count but columns differ):
/// Row A: [x][x][ ][ ]
/// Row B: [ ][ ][x][x]
///
/// Valid (aligned rectangle):
/// Row A: [x][x][ ][ ]
/// Row B: [x][x][ ][ ]
/// </code>
/// </remarks>
public sealed class MisalignedRowsRule : ISeatSelectionRule
{
    /// <summary>
    /// Detects cross-row selections that are not aligned as a rectangular block.
    /// </summary>
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.MisalignedRows);
        // 1. Rule applies only when policy enforces it and selection spans multiple rows.
        if (level == SeatSelectionPolicyLevel.Allow || context.SelectedSeatsByRow.Count <= 1)
        {
            return [];
        }

        // 2. Normalize each selected row into ordered seat column arrays.
        var rows = context.SelectedSeatsByRow
            .OrderBy(x => x.Key)
            .Select(x => new
            {
                x.Key,
                Columns = x.Value.Select(seat => seat.Column).OrderBy(column => column).ToArray()
            })
            .ToArray();

        // 3. Compare every row against the first row baseline shape.
        var baseline = rows[0].Columns;
        foreach (var row in rows.Skip(1))
        {
            if (baseline.Length != row.Columns.Length)
            {
                return BuildViolation(context, level);
            }

            for (var index = 0; index < baseline.Length; index++)
            {
                if (baseline[index] != row.Columns[index])
                {
                    return BuildViolation(context, level);
                }
            }
        }

        return [];
    }

    private static IReadOnlyList<SeatSelectionViolation> BuildViolation(
        SeatSelectionValidationContext context,
        SeatSelectionPolicyLevel level)
    {
        return
        [
            new SeatSelectionViolation(
                Type: SeatSelectionViolationType.MisalignedRows,
                Level: level,
                Message: "Selected seats across rows should align in a rectangular block.",
                AffectedSeats: context.SelectedSeats.Select(x => x.Code).OrderBy(x => x).ToList())
        ];
    }
}
