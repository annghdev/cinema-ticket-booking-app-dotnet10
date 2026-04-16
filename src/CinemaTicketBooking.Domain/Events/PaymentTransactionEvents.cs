namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a payment transaction is created for a booking.
/// </summary>
public record PaymentTransactionCreated(
    Guid TransactionId,
    Guid BookingId,
    PaymentMethod Method,
    decimal Amount,
    PaymentRedirectBehavior RedirectBehavior) : BaseDomainEvent;

/// <summary>
/// Raised when a payment transaction is confirmed as successful.
/// Side effects: confirm booking, send receipt email, generate QR code.
/// </summary>
public record PaymentTransactionSucceeded(
    Guid TransactionId,
    Guid BookingId,
    PaymentMethod Method,
    string GatewayTransactionId,
    decimal Amount) : BaseDomainEvent;

/// <summary>
/// Raised when a payment transaction fails or is rejected by the gateway.
/// Side effects: notify customer, release tickets if no retry.
/// </summary>
public record PaymentTransactionFailed(
    Guid TransactionId,
    Guid BookingId,
    PaymentMethod Method,
    string? Reason) : BaseDomainEvent;
