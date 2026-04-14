using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Schedules auto-release when a ticket is locked.
/// </summary>
public class TicketLockedHandler(IMessageBus bus, IOptions<TicketLockingOptions> options)
{
    /// <summary>
    /// Persists delayed timeout release through Wolverine scheduling.
    /// </summary>
    public async Task Handle(TicketLocked domainEvent, CancellationToken ct)
    {
        var executeAt = DateTimeOffset.UtcNow.Add(options.Value.LockHoldDuration);
        await bus.ScheduleAsync(
            new ReleaseTicketLockByTimeoutCommand
            {
                TicketId = domainEvent.TicketId,
                LockingBy = domainEvent.LockingBy,
                CorrelationId = domainEvent.TicketId.ToString()
            },
            executeAt);
    }
}
