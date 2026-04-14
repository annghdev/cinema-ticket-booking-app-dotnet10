namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a ticket is locked by a customer (Available → Locking).
/// Side effects: push real-time seat status via SignalR, start lock expiry timer.
/// </summary>
public record TicketLocked(
    Guid TicketId,
    Guid ShowTimeId,
    string TicketCode,
    string LockingBy,
    decimal Price) : BaseDomainEvent;

/// <summary>
/// Raised when a ticket enters pending payment state (Locking → PendingPayment).
/// Side effects: schedule payment-timeout auto release and push seat reservation updates.
/// </summary>
public record TicketPendingPayment(
    Guid TicketId,
    Guid ShowTimeId,
    Guid BookingId,
    string TicketCode,
    DateTimeOffset PaymentExpiresAt,
    decimal Price) : BaseDomainEvent;

/// <summary>
/// Raised when a ticket is released back to available.
/// Side effects: push real-time seat availability via SignalR.
/// </summary>
public record TicketReleased(
    Guid TicketId,
    Guid ShowTimeId,
    string TicketCode) : BaseDomainEvent;

/// <summary>
/// Raised when a ticket is marked as sold (PendingPayment → Sold).
/// Side effects: real-time seat status update via SignalR, analytics.
/// </summary>
public record TicketSold(
    Guid TicketId,
    Guid ShowTimeId,
    Guid BookingId,
    string TicketCode,
    decimal Price) : BaseDomainEvent;
