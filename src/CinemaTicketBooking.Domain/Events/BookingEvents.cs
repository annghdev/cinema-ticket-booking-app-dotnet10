namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a booking is confirmed (Pending → Confirmed).
/// Side effects: send confirmation email/SMS, generate QR code, push notification.
/// </summary>
public record BookingConfirmed(
    Guid BookingId,
    Guid ShowTimeId,
    Guid? CustomerId,
    string CustomerName,
    string Email,
    string PhoneNumber,
    decimal FinalAmount,
    int TicketCount,
    DateTimeOffset ShowTimeStartAt) : IDomainEvent;

/// <summary>
/// Raised when a booking is cancelled (Pending/Confirmed → Cancelled).
/// Side effects: send cancellation email, refund processing, real-time seat update via SignalR.
/// </summary>
public record BookingCancelled(
    Guid BookingId,
    Guid ShowTimeId,
    Guid? CustomerId,
    string CustomerName,
    string Email,
    string PhoneNumber,
    decimal FinalAmount,
    BookingStatus PreviousStatus,
    List<Guid> ReleasedTicketIds) : IDomainEvent;

/// <summary>
/// Raised when a customer checks in at the cinema (Confirmed → CheckedIn).
/// Side effects: analytics tracking, loyalty points accumulation.
/// </summary>
public record BookingCheckedIn(
    Guid BookingId,
    Guid ShowTimeId,
    Guid? CustomerId,
    string CustomerName,
    int TicketCount,
    decimal FinalAmount) : IDomainEvent;
