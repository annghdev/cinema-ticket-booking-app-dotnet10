using System.Collections.Concurrent;
using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fakes;

/// <summary>
/// In-memory probe for realtime payment notifications.
/// </summary>
public sealed class InMemoryPaymentRealtimePublisher : IPaymentRealtimePublisher
{
    private readonly ConcurrentQueue<PaymentConfirmedRealtimeEvent> _events = new();

    public IReadOnlyCollection<PaymentConfirmedRealtimeEvent> Events => _events.ToArray();

    public Task PublishPaymentConfirmedAsync(PaymentConfirmedRealtimeEvent @event, CancellationToken ct)
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
