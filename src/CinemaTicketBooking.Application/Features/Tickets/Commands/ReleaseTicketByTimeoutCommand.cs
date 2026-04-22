namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Scheduled command to release locking ticket when lock timeout expires.
/// </summary>
public class ReleaseTicketLockByTimeoutCommand : ICommand
{
    public Guid TicketId { get; set; }
    public string LockingBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles delayed auto-release for timed-out locking stage.
/// </summary>
public class ReleaseTicketLockByTimeoutHandler(IUnitOfWork uow, ITicketLocker locker, ITicketRealtimePublisher realtimePublisher)
{
    /// <summary>
    /// Performs idempotent release only when ticket is still locked by the same owner.
    /// </summary>
    public async Task Handle(ReleaseTicketLockByTimeoutCommand cmd, CancellationToken ct)
    {
        var ticket = await uow.Tickets.GetByIdAsync(cmd.TicketId, ct);
        var now = DateTimeOffset.UtcNow;
        if (ticket is null
            || ticket.Status != TicketStatus.Locking
            || ticket.LockingBy != cmd.LockingBy
            || !ticket.LockExpiresAt.HasValue
            || ticket.LockExpiresAt > now)
        {
            return;
        }

        ticket.Release();
        uow.Tickets.Update(ticket);
        await uow.CommitAsync(ct);
        await locker.ReleaseAsync(cmd.TicketId, cmd.LockingBy, ct);
    }
}
