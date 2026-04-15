namespace CinemaTicketBooking.Domain;

public sealed class MisalignedRowsRule : ISeatSelectionRule
{
    public IReadOnlyList<SeatSelectionViolation> Evaluate(SeatSelectionValidationContext context)
    {
        var level = context.Policy.ResolveLevel(SeatSelectionViolationType.MisalignedRows);
        if (level == SeatSelectionPolicyLevel.Allow || context.SelectedSeatsByRow.Count <= 1)
        {
            return [];
        }

        var rows = context.SelectedSeatsByRow
            .OrderBy(x => x.Key)
            .Select(x => new
            {
                x.Key,
                Columns = x.Value.Select(seat => seat.Column).OrderBy(column => column).ToArray()
            })
            .ToArray();

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
