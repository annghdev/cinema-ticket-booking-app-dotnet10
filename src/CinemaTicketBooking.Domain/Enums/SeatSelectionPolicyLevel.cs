namespace CinemaTicketBooking.Domain;

/// <summary>
/// Defines how a seat-selection rule should be enforced.
/// </summary>
public enum SeatSelectionPolicyLevel
{
    Allow = 0,
    Warning = 1,
    Block = 2
}
