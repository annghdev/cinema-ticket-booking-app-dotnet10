using System.Text.Json;

namespace CinemaTicketBooking.Domain;

/// <summary>
/// Validates selected tickets against seat-layout policy before checkout proceeds.
/// </summary>
public sealed class SeatSelectionValidator
{
    private readonly IReadOnlyList<ISeatSelectionRule> _rules;

    public SeatSelectionValidator(IEnumerable<ISeatSelectionRule> rules)
    {
        _rules = rules.ToList();
    }

    public static SeatSelectionValidator CreateDefault()
    {
        return new SeatSelectionValidator(
        [
            new MaxTicketsPerCheckoutRule(),
            new MaxRowsPerCheckoutRule(),
            new SplitAcrossAisleRule(),
            new MisalignedRowsRule(),
            new OrphanSeatRule(),
            new CheckerboardRule(),
            new IsolatedRowEndSingleRule()
        ]);
    }

    /// <summary>
    /// Executes rule chain and returns warnings/errors with seat hints.
    /// </summary>
    public SeatSelectionValidationResult Validate(
        ShowTime showTime,
        SeatSelectionPolicy policy,
        IReadOnlyCollection<Guid> selectedTicketIds,
        string customerSessionId)
    {
        if (showTime.Screen is null)
        {
            throw new InvalidOperationException("Showtime screen data is required for seat-layout validation.");
        }

        if (showTime.Screen.Seats.Count == 0)
        {
            throw new InvalidOperationException("Screen seat data is required for seat-layout validation.");
        }

        var result = new SeatSelectionValidationResult();
        if (selectedTicketIds.Count == 0)
        {
            result.AddViolation(new SeatSelectionViolation(
                Type: SeatSelectionViolationType.TicketUnavailable,
                Level: SeatSelectionPolicyLevel.Block,
                Message: "At least one ticket must be selected.",
                AffectedSeats: []));
            result.Hints.Add("Select one or more seats before checkout.");
            return result;
        }

        var seatByCode = showTime.Screen.Seats.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);
        var selectedTickets = new List<Ticket>(selectedTicketIds.Count);
        var selectedSeats = new List<Seat>(selectedTicketIds.Count);
        var selectedSeatCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var ticketById = showTime.Tickets.ToDictionary(x => x.Id, x => x);
        foreach (var ticketId in selectedTicketIds)
        {
            if (!ticketById.TryGetValue(ticketId, out var ticket))
            {
                result.AddViolation(new SeatSelectionViolation(
                    Type: SeatSelectionViolationType.TicketUnavailable,
                    Level: SeatSelectionPolicyLevel.Block,
                    Message: $"Ticket '{ticketId}' does not exist.",
                    AffectedSeats: []));
                continue;
            }

            if (!CanUseTicket(ticket, customerSessionId))
            {
                var seatCode = ticket.SeatCode;
                result.AddViolation(new SeatSelectionViolation(
                    Type: SeatSelectionViolationType.TicketUnavailable,
                    Level: SeatSelectionPolicyLevel.Block,
                    Message: $"Seat '{seatCode}' is no longer available for checkout.",
                    AffectedSeats: string.IsNullOrWhiteSpace(seatCode) ? [] : [seatCode]));
                continue;
            }

            var selectedSeatCode = ticket.SeatCode;
            if (string.IsNullOrWhiteSpace(selectedSeatCode) || !seatByCode.TryGetValue(selectedSeatCode, out var seat))
            {
                result.AddViolation(new SeatSelectionViolation(
                    Type: SeatSelectionViolationType.TicketUnavailable,
                    Level: SeatSelectionPolicyLevel.Block,
                    Message: $"Cannot resolve seat position for ticket '{ticket.Code}'.",
                    AffectedSeats: []));
                continue;
            }

            selectedTickets.Add(ticket);
            selectedSeats.Add(seat);
            selectedSeatCodes.Add(seat.Code);
        }

        if (result.Errors.Count > 0)
        {
            PopulateHints(result);
            return result;
        }

        var occupiedSeatCodes = ResolveOccupiedSeatCodes(showTime, customerSessionId);
        var aisleColumnsByRow = ParseAisleColumnsByRow(showTime.Screen.SeatMap);
        var activeSeatsByRow = showTime.Screen.Seats
            .Where(x => x.IsActive)
            .GroupBy(x => x.Row)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Seat>)group.OrderBy(seat => seat.Column).ToList());
        var selectedSeatsByRow = selectedSeats
            .GroupBy(x => x.Row)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Seat>)group.OrderBy(seat => seat.Column).ToList());

        var context = new SeatSelectionValidationContext
        {
            Screen = showTime.Screen,
            ShowTime = showTime,
            Policy = policy,
            CustomerSessionId = customerSessionId,
            SelectedTickets = selectedTickets,
            SelectedSeats = selectedSeats,
            SelectedSeatCodes = selectedSeatCodes,
            OccupiedSeatCodes = occupiedSeatCodes,
            AisleColumnsByRow = aisleColumnsByRow,
            ActiveSeatsByRow = activeSeatsByRow,
            SelectedSeatsByRow = selectedSeatsByRow
        };

        foreach (var rule in _rules)
        {
            foreach (var violation in rule.Evaluate(context))
            {
                result.AddViolation(violation);
            }
        }

        PopulateHints(result);
        return result;
    }

    private static bool CanUseTicket(Ticket ticket, string customerSessionId)
    {
        return ticket.Status switch
        {
            TicketStatus.Available => true,
            TicketStatus.Locking => ticket.LockingBy == customerSessionId,
            _ => false
        };
    }

    private static HashSet<string> ResolveOccupiedSeatCodes(ShowTime showTime, string customerSessionId)
    {
        var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ticket in showTime.Tickets)
        {
            var seatCode = ticket.SeatCode;
            if (string.IsNullOrWhiteSpace(seatCode))
            {
                continue;
            }

            var shouldOccupy = ticket.Status is TicketStatus.Sold or TicketStatus.PendingPayment
                               || (ticket.Status == TicketStatus.Locking && ticket.LockingBy != customerSessionId);
            if (shouldOccupy)
            {
                occupied.Add(seatCode);
            }
        }

        return occupied;
    }

    private static Dictionary<int, IReadOnlyList<int>> ParseAisleColumnsByRow(string seatMap)
    {
        var seatArray = ParseSeatMapArray(seatMap);
        var result = new Dictionary<int, IReadOnlyList<int>>();
        for (var row = 0; row < seatArray.GetLength(0); row++)
        {
            var aisles = new List<int>();
            for (var col = 0; col < seatArray.GetLength(1); col++)
            {
                if (seatArray[row, col] == 0)
                {
                    aisles.Add(col + 1);
                }
            }

            result[row + 1] = aisles;
        }

        return result;
    }

    private static int[,] ParseSeatMapArray(string seatMap)
    {
        if (string.IsNullOrWhiteSpace(seatMap))
        {
            return new int[0, 0];
        }

        var normalized = seatMap.Trim();
        if (normalized.StartsWith("[", StringComparison.Ordinal))
        {
            var array = JsonSerializer.Deserialize<int[][]>(normalized)
                ?? throw new FormatException("SeatMap JSON is invalid.");
            if (array.Length == 0)
            {
                return new int[0, 0];
            }

            var columns = array[0].Length;
            var result = new int[array.Length, columns];
            for (var row = 0; row < array.Length; row++)
            {
                if (array[row].Length != columns)
                {
                    throw new FormatException($"SeatMap row {row + 1} does not have {columns} columns.");
                }

                for (var col = 0; col < columns; col++)
                {
                    result[row, col] = array[row][col];
                }
            }

            return result;
        }

        var lines = normalized.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var firstColumns = lines[0].Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Length;
        var plainResult = new int[lines.Length, firstColumns];

        for (var row = 0; row < lines.Length; row++)
        {
            var tokens = lines[row].Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != firstColumns)
            {
                throw new FormatException($"SeatMap row {row + 1} does not have {firstColumns} columns.");
            }

            for (var col = 0; col < tokens.Length; col++)
            {
                if (!int.TryParse(tokens[col], out var value))
                {
                    throw new FormatException($"SeatMap value at row {row + 1}, column {col + 1} is invalid.");
                }

                plainResult[row, col] = value;
            }
        }

        return plainResult;
    }

    private static void PopulateHints(SeatSelectionValidationResult result)
    {
        if (result.Errors.Count == 0 && result.Warnings.Count == 0)
        {
            return;
        }

        var allViolations = result.Errors.Concat(result.Warnings).ToList();
        if (allViolations.Any(x => x.Type == SeatSelectionViolationType.OrphanSeat))
        {
            result.Hints.Add("Avoid leaving a single empty seat between occupied seats.");
        }

        if (allViolations.Any(x => x.Type == SeatSelectionViolationType.SplitAcrossAisle))
        {
            result.Hints.Add("Keep seats in the same row and same aisle segment when possible.");
        }

        if (allViolations.Any(x => x.Type == SeatSelectionViolationType.MisalignedRows))
        {
            result.Hints.Add("If selecting two rows, align seats into a rectangular block.");
        }

        if (allViolations.Any(x => x.Type == SeatSelectionViolationType.Checkerboard))
        {
            result.Hints.Add("Avoid alternating seat patterns that create checkerboard gaps.");
        }
    }
}
