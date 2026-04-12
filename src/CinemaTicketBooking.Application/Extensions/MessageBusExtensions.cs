using CinemaTicketBooking.Domain;
using Wolverine;

namespace CinemaTicketBooking.Application.Extensions;

public static class MessageBusExtensions
{
    public static async Task DispatchEventsAsync(
        this IMessageBus messageBus,
        params BaseEntity[] entities)
    {
        var events = entities
            .SelectMany(e => e.Events)
            .ToArray();

        foreach (var @event in events)
        {
            await messageBus.PublishAsync(@event);
        }
    }
}
