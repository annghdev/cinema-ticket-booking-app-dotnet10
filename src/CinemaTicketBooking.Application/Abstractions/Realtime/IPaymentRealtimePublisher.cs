namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Publishes realtime payment status changes to connected clients.
/// </summary>
public interface IPaymentRealtimePublisher
{
    /// <summary>
    /// Pushes booking payment confirmation update to interested clients.
    /// </summary>
    Task PublishPaymentConfirmedAsync(PaymentConfirmedRealtimeEvent @event, CancellationToken ct);
}

/// <summary>
/// Event payload emitted when a booking payment is confirmed.
/// </summary>
public sealed record PaymentConfirmedRealtimeEvent(
    Guid BookingId,
    Guid PaymentTransactionId,
    string GatewayTransactionId,
    string Status,
    string? CheckinQrCode,
    DateTimeOffset OccurredAtUtc);
