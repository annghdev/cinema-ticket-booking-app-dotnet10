using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Domain;

/// <summary>
/// Booking represents a customer's reservation for a specific ShowTime.
/// It aggregates tickets and concessions, tracks pricing (original and discounted),
/// and enforces state transitions: Pending → Confirmed → CheckedIn, or → Cancelled.
/// </summary>
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

    /// <summary>
    /// Total price before any discount is applied.
    /// </summary>
    public decimal OriginAmount { get; set; }

    /// <summary>
    /// Final price after discount. Calculated by UpdateFinalAmount().
    /// </summary>
    public decimal FinalAmount { get; set; }
    public List<BookingTicket> Tickets { get; set; } = [];
    public List<BookingConcession> Concessions { get; set; } = [];
    public string? QrCode { get; set; }
    public BookingStatus Status { get; set; }

    // =============================================================
    // Add Items: Tickets and Concessions
    // =============================================================

    /// <summary>
    /// Adds a locked ticket to this booking.
    /// The ticket must belong to the same ShowTime and be locked by the current Customer.
    /// </summary>
    public void AddTicket(Ticket ticket)
    {
        // 1. Validate customer exists
        if (Customer == null)
        {
            throw new InvalidOperationException("Customer information is required to add a ticket to the booking.");
        }

        // 2. Validate ticket belongs to the same ShowTime
        if (ticket.ShowTimeId != ShowTimeId)
        {
            throw new InvalidOperationException("Ticket does not belong to the same showtime as the booking.");
        }

        // 3. Validate ticket is in Locking status
        if (ticket.Status != TicketStatus.Locking)
        {
            throw new InvalidOperationException("Ticket is not available for booking.");
        }

        // 4. Validate the ticket is locked by this customer (by SessionId or CustomerId)
        if (!(ticket.LockingBy == Customer.SessionId || ticket.LockingBy == Customer.Id.ToString()))
        {
            throw new InvalidOperationException("Ticket is not available for booking.");
        }

        // 5. Create BookingTicket join entity and accumulate price
        Tickets.Add(new BookingTicket
        {
            Id = Guid.CreateVersion7(),
            BookingId = Id,
            TicketId = ticket.Id,
            Ticket = ticket
        });

        OriginAmount += ticket.Price;
    }

    /// <summary>
    /// Adds a concession item (snack/drink) to this booking with the specified quantity.
    /// </summary>
    public void AddConcession(Concession concession, int quantity)
    {
        // 1. Validate concession is available
        if (!concession.IsAvailable)
        {
            throw new InvalidOperationException("Concession is not available for booking.");
        }

        // 2. Validate quantity is positive
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        // 3. Create BookingConcession join entity and accumulate price
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

    // =============================================================
    // State Transitions: Confirm, Cancel, CheckIn
    // =============================================================

    /// <summary>
    /// Confirms the booking and marks all associated tickets as Sold.
    /// Only Pending bookings can be confirmed.
    /// </summary>
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be confirmed.");
        }
        Status = BookingStatus.Confirmed;

        // Mark each ticket as sold, linking it to this booking
        foreach (var bookingTicket in Tickets)
        {
            if (bookingTicket.Ticket == null)
            {
                throw new InvalidOperationException("Booking ticket must have an associated ticket.");
            }
            bookingTicket.Ticket.MarkAsSold(Id);
        }
    }

    /// <summary>
    /// Cancels the booking and releases all associated tickets back to Available.
    /// Cannot cancel already-cancelled or checked-in bookings.
    /// </summary>
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

        // Release each ticket back to the available pool
        foreach (var bookingTicket in Tickets)
        {
            if (bookingTicket.Ticket == null)
            {
                throw new InvalidOperationException("Booking ticket must have an associated ticket.");
            }
            bookingTicket.Ticket.Release();
        }
    }

    /// <summary>
    /// Marks the booking as checked-in (customer arrived at the cinema).
    /// Only Confirmed bookings can be checked in.
    /// </summary>
    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");
        }
        Status = BookingStatus.CheckedIn;
    }

    // =============================================================
    // Pricing: Apply discount to calculate final amount
    // =============================================================

    /// <summary>
    /// Calculates the final amount after applying a discount.
    /// FinalAmount = max(0, OriginAmount - discountAmount).
    /// </summary>
    public void UpdateFinalAmount(decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
        }
        FinalAmount = Math.Max(0, OriginAmount - discountAmount);
    }
}

/// <summary>
/// Join entity between Booking and Ticket (many-to-many).
/// </summary>
public class BookingTicket : IEntity
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}

/// <summary>
/// Join entity between Booking and Concession (many-to-many with quantity).
/// </summary>
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
