using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CinemaTicketBooking.WebServer.Hubs;

/// <summary>
/// Hub for realtime booking payment updates.
/// </summary>
[AllowAnonymous]
public class PaymentHub : Hub
{
    /// <summary>
    /// SignalR event emitted when payment is confirmed.
    /// </summary>
    public const string PaymentConfirmedEvent = "payment-confirmed";

    /// <summary>
    /// Joins the current connection to a booking payment group.
    /// </summary>
    public Task SubscribeBooking(Guid bookingId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildBookingGroup(bookingId));
    }

    /// <summary>
    /// Leaves the current connection from a booking payment group.
    /// </summary>
    public Task UnsubscribeBooking(Guid bookingId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildBookingGroup(bookingId));
    }

    /// <summary>
    /// Returns canonical payment group name for a booking.
    /// </summary>
    public static string BuildBookingGroup(Guid bookingId) => $"booking-payment:{bookingId:N}";
}
