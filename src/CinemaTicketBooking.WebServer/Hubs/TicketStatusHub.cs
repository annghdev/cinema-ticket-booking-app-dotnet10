using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CinemaTicketBooking.WebServer.Hubs;

/// <summary>
/// Hub for realtime ticket status updates per showtime.
/// </summary>
[AllowAnonymous]
public class TicketStatusHub : Hub
{
    /// <summary>
    /// SignalR event name for ticket status delta updates.
    /// </summary>
    public const string TicketStatusChangedEvent = "ticket-status-changed";

    /// <summary>
    /// Joins the current connection to a showtime group.
    /// </summary>
    public Task SubscribeShowTime(Guid showTimeId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildShowTimeGroup(showTimeId));
    }

    /// <summary>
    /// Leaves the current connection from a showtime group.
    /// </summary>
    public Task UnsubscribeShowTime(Guid showTimeId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildShowTimeGroup(showTimeId));
    }

    /// <summary>
    /// Returns canonical showtime group name.
    /// </summary>
    public static string BuildShowTimeGroup(Guid showTimeId) => $"showtime:{showTimeId:N}";
}
