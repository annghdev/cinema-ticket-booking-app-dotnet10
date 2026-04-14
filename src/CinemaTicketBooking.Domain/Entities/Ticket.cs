using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Domain;

/// <summary>
/// Ticket represents a single seat for a specific ShowTime.
/// Lifecycle: Available → Locking (customer selects) → Sold (booking confirmed).
/// A locked ticket can be released back to Available if the customer abandons it.
/// </summary>
public class Ticket : AggregateRoot
{
    /// <summary>
    /// Unique ticket code. Format: "{Date:yyyyMMdd}-{ScreenCode}-{SeatCode}"
    /// Example: "20260415-S1-A3"
    /// </summary>
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Final price calculated from PricingPolicy at ShowTime creation time.
    /// </summary>
    public decimal Price { get; set; }
    public Guid ShowTimeId { get; set; }
    [JsonIgnore]
    public ShowTime? ShowTime { get; set; }

    /// <summary>
    /// Identifier of who is currently locking this ticket (Customer.SessionId or Customer.Id).
    /// Null when the ticket is Available or Sold.
    /// </summary>
    public string? LockingBy { get; set; }
    public Guid? BookingId { get; set; }
    public TicketStatus Status { get; set; }

    // =============================================================
    // State Transitions: Lock, Release, MarkAsSold
    // =============================================================

    /// <summary>
    /// Locks the ticket for a specific customer (prevents others from selecting it).
    /// Only Available tickets can be locked.
    /// </summary>
    public void Lock(string lockBy)
    {
        if (Status != TicketStatus.Available)
            throw new InvalidOperationException("Only available tickets can be locked.");
        Status = TicketStatus.Locking;
        LockingBy = lockBy;

        RaiseEvent(new TicketLocked(
            TicketId: Id,
            ShowTimeId: ShowTimeId,
            TicketCode: Code,
            LockingBy: lockBy,
            Price: Price));
    }

    /// <summary>
    /// Releases the ticket unconditionally (used during booking cancellation).
    /// Resets status to Available and clears the lock owner.
    /// </summary>
    public void Release()
    {
        Status = TicketStatus.Available;
        LockingBy = null;

        RaiseEvent(new TicketReleased(
            TicketId: Id,
            ShowTimeId: ShowTimeId,
            TicketCode: Code));
    }

    /// <summary>
    /// Releases the ticket with ownership check (used when a customer manually deselects).
    /// Only the customer who locked the ticket can release it.
    /// </summary>
    public void Release(string releaseBy)
    {
        if (Status == TicketStatus.Available)
            throw new InvalidOperationException("Only locking or sold tickets can be released.");

        if (LockingBy != releaseBy)
            throw new InvalidOperationException("Only locking tickets by the same customer can be released.");
        Status = TicketStatus.Available;
        LockingBy = null;

        RaiseEvent(new TicketReleased(
            TicketId: Id,
            ShowTimeId: ShowTimeId,
            TicketCode: Code));
    }

    /// <summary>
    /// Marks the ticket as sold and links it to a confirmed Booking.
    /// Only Locking tickets can be marked as sold.
    /// </summary>
    public void MarkAsSold(Guid bookingId)
    {
        if (Status != TicketStatus.Locking)
            throw new InvalidOperationException("Only locking tickets can be marked as sold.");
        Status = TicketStatus.Sold;
        BookingId = bookingId;
        LockingBy = null;

        RaiseEvent(new TicketSold(
            TicketId: Id,
            ShowTimeId: ShowTimeId,
            BookingId: bookingId,
            TicketCode: Code,
            Price: Price));
    }
}

//public enum TicketStatus
//{
//    Available,
//    Locking,
//    Booked
//}
