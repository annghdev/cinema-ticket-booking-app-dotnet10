using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Startup command that recovers stale locking and pending-payment tickets.
/// </summary>
public class RecoverStaleTicketLocksCommand : ICommand
{
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles startup stale-ticket recovery to self-heal missing schedules.
/// </summary>
public class RecoverStaleTicketLocksHandler(
    IUnitOfWork uow,
    IMessageBus bus,
    IOptions<TicketLockingOptions> options)
{
    /// <summary>
    /// Enqueues timeout-release commands for stale locking and pending-payment tickets.
    /// </summary>
    public async Task Handle(RecoverStaleTicketLocksCommand cmd, CancellationToken ct)
    {
        var staleThreshold = DateTimeOffset.UtcNow;
        var batchSize = options.Value.RecoveryBatchSize;

        var staleLocks = await uow.Tickets
            .GetQueryFilter()
            .Where(x => x.Status == TicketStatus.Locking
                        && x.LockExpiresAt.HasValue
                        && x.LockExpiresAt <= staleThreshold
                        && !string.IsNullOrWhiteSpace(x.LockingBy))
            .OrderBy(x => x.LockExpiresAt)
            .Select(x => new { x.Id, x.LockingBy, x.LockExpiresAt })
            .Take(batchSize)
            .ToListAsync(ct);

        foreach (var staleLock in staleLocks)
        {
            ct.ThrowIfCancellationRequested();
            await bus.PublishAsync(new ReleaseTicketLockByTimeoutCommand
            {
                TicketId = staleLock.Id,
                LockingBy = staleLock.LockingBy!,
                CorrelationId = cmd.CorrelationId
            });
        }

        var stalePendingPayments = await uow.Tickets
            .GetQueryFilter()
            .Where(x => x.Status == TicketStatus.PendingPayment
                        && x.PaymentExpiresAt.HasValue
                        && x.PaymentExpiresAt <= staleThreshold
                        && x.BookingId.HasValue)
            .OrderBy(x => x.PaymentExpiresAt)
            .Select(x => new { x.Id, x.BookingId, x.PaymentExpiresAt })
            .Take(batchSize)
            .ToListAsync(ct);

        foreach (var stalePendingPayment in stalePendingPayments)
        {
            ct.ThrowIfCancellationRequested();
            await bus.PublishAsync(new ReleaseTicketPaymentByTimeoutCommand
            {
                TicketId = stalePendingPayment.Id,
                BookingId = stalePendingPayment.BookingId!.Value,
                CorrelationId = cmd.CorrelationId
            });
        }
    }
}
