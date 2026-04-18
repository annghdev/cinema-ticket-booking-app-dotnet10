using CinemaTicketBooking.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace CinemaTicketBooking.WebServer.Hubs;

/// <summary>
/// SignalR-backed publisher for ticket status delta updates.
/// </summary>
public class SignalRTicketRealtimePublisher(IHubContext<TicketStatusHub> hubContext) : ITicketRealtimePublisher
{
    /// <summary>
    /// Sends ticket status delta to all clients subscribed to the showtime group.
    /// </summary>
    public async Task PublishTicketStatusChangedAsync(TicketStatusChangedRealtimeEvent @event, CancellationToken ct)
    {
        var groupName = TicketStatusHub.BuildShowTimeGroup(@event.ShowTimeId);
        await hubContext.Clients.Group(groupName).SendAsync(TicketStatusHub.TicketStatusChangedEvent, @event, ct);
    }
}
