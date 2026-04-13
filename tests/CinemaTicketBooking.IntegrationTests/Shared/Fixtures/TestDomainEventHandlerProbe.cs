using CinemaTicketBooking.Domain;
using System.Collections.Concurrent;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public sealed class TestDomainEventHandlerProbe
{
    private readonly ConcurrentQueue<IDomainEvent> _handledEvents = [];
    private TaskCompletionSource<IDomainEvent> _handledEventSource = NewSource();

    public IReadOnlyCollection<IDomainEvent> HandledEvents => _handledEvents.ToArray();

    public void Reset()
    {
        while (_handledEvents.TryDequeue(out _))
        {
        }

        _handledEventSource = NewSource();
    }

    public void MarkHandled(IDomainEvent domainEvent)
    {
        _handledEvents.Enqueue(domainEvent);
        _handledEventSource.TrySetResult(domainEvent);
    }

    public async Task<IDomainEvent> WaitForHandledEventAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        return await _handledEventSource.Task.WaitAsync(timeoutCts.Token);
    }

    private static TaskCompletionSource<IDomainEvent> NewSource()
    {
        return new TaskCompletionSource<IDomainEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
