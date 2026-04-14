using CinemaTicketBooking.Application.Features;
using Wolverine;

namespace CinemaTicketBooking.WebServer.CronJobs;

/// <summary>
/// Triggers stale ticket reservation recovery on service startup.
/// </summary>
public class TicketLockRecoveryHostedService(IMessageBus bus) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await bus.PublishAsync(new RecoverStaleTicketLocksCommand
        {
            CorrelationId = Guid.CreateVersion7().ToString()
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
