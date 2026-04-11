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
}

//public enum TicketStatus
//{
//    Available,
//    Locking,
//    Booked
//}
