namespace CinemaTicketBooking.Domain;

/// <summary>
/// Immutable context passed to seat-selection rules.
/// </summary>
public sealed class SeatSelectionValidationContext
{
    public required Screen Screen { get; init; }
    public required ShowTime ShowTime { get; init; }
    public required SeatSelectionPolicy Policy { get; init; }
    public required string CustomerSessionId { get; init; }
    public required IReadOnlyList<Ticket> SelectedTickets { get; init; }
    public required IReadOnlyList<Seat> SelectedSeats { get; init; }
    public required IReadOnlyDictionary<int, IReadOnlyList<Seat>> SelectedSeatsByRow { get; init; }
    public required IReadOnlySet<string> SelectedSeatCodes { get; init; }
    public required IReadOnlySet<string> OccupiedSeatCodes { get; init; }
    public required IReadOnlyDictionary<int, IReadOnlyList<int>> AisleColumnsByRow { get; init; }
    public required IReadOnlyDictionary<int, IReadOnlyList<Seat>> ActiveSeatsByRow { get; init; }
}
