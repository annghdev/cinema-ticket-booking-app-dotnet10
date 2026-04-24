using CinemaTicketBooking.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace CinemaTicketBooking.WebServer.Hubs;

/// <summary>
/// SignalR-backed publisher for booking payment confirmation updates.
/// </summary>
public sealed class SignalRPaymentRealtimePublisher(IHubContext<PaymentHub> hubContext) : IPaymentRealtimePublisher
{
    /// <summary>
    /// Sends confirmed payment payload to all clients subscribed to the booking group.
    /// </summary>
    public async Task PublishPaymentConfirmedAsync(PaymentConfirmedRealtimeEvent @event, CancellationToken ct)
    {
        var groupName = PaymentHub.BuildBookingGroup(@event.BookingId);
        await hubContext.Clients.Group(groupName).SendAsync(PaymentHub.PaymentConfirmedEvent, @event, ct);
    }
}
