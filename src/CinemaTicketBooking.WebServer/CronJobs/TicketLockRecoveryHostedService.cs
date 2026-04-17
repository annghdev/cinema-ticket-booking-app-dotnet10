using CinemaTicketBooking.Application.Features;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CinemaTicketBooking.WebServer.CronJobs;

/// <summary>
/// Triggers stale ticket reservation recovery on service startup.
/// </summary>
public class TicketLockRecoveryHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var scope = scopeFactory.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
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
