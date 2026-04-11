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

    public void AddTicket(Ticket ticket)
    {
        if (Customer == null)
        {
            throw new InvalidOperationException("Customer information is required to add a ticket to the booking.");
        }
        if (ticket.ShowTimeId != ShowTimeId)
        {
            throw new InvalidOperationException("Ticket does not belong to the same showtime as the booking.");
        }

        if (ticket.Status != TicketStatus.Locking)
        {
            throw new InvalidOperationException("Ticket is not available for booking.");
        }

        if (!(ticket.LockingBy == Customer.SessionId || ticket.LockingBy == Customer.Id.ToString()))
        {
            throw new InvalidOperationException("Ticket is not available for booking.");
        }

        Tickets.Add(new BookingTicket
        {
            Id = Guid.CreateVersion7(),
            BookingId = Id,
            TicketId = ticket.Id,
            Ticket = ticket
        });

        OriginAmount += ticket.Price;
    }

    public void AddConcession(Concession concession, int quantity)
    {
        if (!concession.IsAvailable)
        {
            throw new InvalidOperationException("Concession is not available for booking.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Concessions.Add(new BookingConcession
        {
            Id = Guid.CreateVersion7(),
            BookingId = Id,
            ConcessionId = concession.Id,
            Concession = concession,
            Quantity = quantity
        });
        OriginAmount += concession.Price * quantity;
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be confirmed.");
        }
        Status = BookingStatus.Confirmed;

        foreach (var bookingTicket in Tickets)
        {
            if (bookingTicket.Ticket == null)
            {
                throw new InvalidOperationException("Booking ticket must have an associated ticket.");
            }
            bookingTicket.Ticket.MarkAsSold(Id);
        }
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }

        if(Status == BookingStatus.CheckedIn)
        {
            throw new InvalidOperationException("Checked-in bookings cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;

        foreach (var bookingTicket in Tickets)
        {
            if (bookingTicket.Ticket == null)
            {
                throw new InvalidOperationException("Booking ticket must have an associated ticket.");
            }
            bookingTicket.Ticket.Release();
        }
    }

    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");
        }
        Status = BookingStatus.CheckedIn;
    }

    public void UpdateFinalAmount(decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
        }
        FinalAmount = Math.Max(0, OriginAmount - discountAmount);
    }
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
