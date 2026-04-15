namespace CinemaTicketBooking.Domain;

/// <summary>
/// Represents supported pre-checkout seat-layout violations.
/// </summary>
public enum SeatSelectionViolationType
{
    MaxTickets = 1,
    MaxRows = 2,
    OrphanSeat = 3,
    Checkerboard = 4,
    SplitAcrossAisle = 5,
    IsolatedRowEndSingle = 6,
    MisalignedRows = 7,
    TicketUnavailable = 8
}
