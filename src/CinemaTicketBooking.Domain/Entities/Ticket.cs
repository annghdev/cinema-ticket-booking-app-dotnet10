using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Domain;

public class Ticket : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid ShowTimeId { get; set; }
    [JsonIgnore]
    public ShowTime? ShowTime { get; set; }
    public string? LockingBy { get; set; }
    public Guid? BookingId { get; set; }
    public TicketStatus Status { get; set; }

    public void Lock(string lockBy)
    {
        if (Status != TicketStatus.Available)
            throw new InvalidOperationException("Only available tickets can be locked.");
        Status = TicketStatus.Locking;
        LockingBy = lockBy;
    }

    public void Release()
    {
        Status = TicketStatus.Available;
        LockingBy = null;
    }

    public void Release(string releaseBy)
    {
        if (Status == TicketStatus.Available)
            throw new InvalidOperationException("Only locking or sold tickets can be released.");

        if (LockingBy != releaseBy)
            throw new InvalidOperationException("Only locking tickets by the same customer can be released.");
        Status = TicketStatus.Available;
        LockingBy = null;
    }

    public void MarkAsSold(Guid bookingId)
    {
        if (Status != TicketStatus.Locking)
            throw new InvalidOperationException("Only locking tickets can be marked as sold.");
        Status = TicketStatus.Sold;
        BookingId = bookingId;
        LockingBy = null;
    }
}

//public enum TicketStatus
//{
//    Available,
//    Locking,
//    Booked
//}
