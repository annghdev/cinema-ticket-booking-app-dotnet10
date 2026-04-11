using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Domain;

public class Booking : AuditableEntity
{
    public Guid ShowTimeId { get; set; }
    public ShowTime? ShowTime { get; set; }
    public Guid? CustomerId { get; set; }
    [JsonIgnore]
    public Customer? Customer { get; set; }
    public required string CustomerName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal OriginAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<BookingTicket> Tickets { get; set; } = [];
    public List<BookingConcession> Concessions { get; set; } = [];
    public string? QrCode { get; set; }
    public BookingStatus Status { get; set; }
}

public class BookingTicket : IEntity
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}

public class BookingConcession : IEntity
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ConcessionId { get; set; }
    public Concession? Concession { get; set; }
    public int Quantity { get; set; }
}

//public enum BookingStatus
//{
//    Initializing,
//    Pending,
//    Confirmed,
//    CheckedIn,
//    Cancelled,
//}
