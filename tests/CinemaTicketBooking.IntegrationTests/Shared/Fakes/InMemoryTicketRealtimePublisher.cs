using System.Collections.Concurrent;
using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fakes;

/// <summary>
/// In-memory probe for realtime ticket status notifications.
/// </summary>
public sealed class InMemoryTicketRealtimePublisher : ITicketRealtimePublisher
{
    private readonly ConcurrentQueue<TicketStatusChangedRealtimeEvent> _events = new();

    public IReadOnlyCollection<TicketStatusChangedRealtimeEvent> Events => _events.ToArray();

    public Task PublishTicketStatusChangedAsync(TicketStatusChangedRealtimeEvent @event, CancellationToken ct)
    {
        _events.Enqueue(@event);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        while (_events.TryDequeue(out _))
        {
        }
    }
}
