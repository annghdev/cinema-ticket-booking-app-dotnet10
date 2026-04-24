using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Infrastructure.Payments;

/// <summary>
/// No-op realtime publisher used outside web host SignalR runtime.
/// </summary>
public sealed class NoOpPaymentRealtimePublisher : IPaymentRealtimePublisher
{
    /// <summary>
    /// Intentionally does nothing.
    /// </summary>
    public Task PublishPaymentConfirmedAsync(PaymentConfirmedRealtimeEvent @event, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
