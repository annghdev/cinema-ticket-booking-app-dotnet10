using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Locks a ticket for a specific customer/session.
/// </summary>
public class LockTicketCommand : ICommand
{
    public Guid TicketId { get; set; }
    public string LockBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles ticket lock requests.
/// </summary>
public class LockTicketHandler(
    IUnitOfWork uow,
    ITicketLocker locker,
    IOptions<TicketLockingOptions> options)
{
    /// <summary>
    /// Attempts distributed lock first, then persists ticket lock transition.
    /// </summary>
    public async Task Handle(LockTicketCommand cmd, CancellationToken ct)
    {
        // 1. Resolve lock timeout
        var timeout = options.Value.LockHoldDuration;
        var lockExpiresAt = DateTimeOffset.UtcNow.Add(timeout);

        // 2. Acquire lock (Redis first, DB fallback is encapsulated in infrastructure).
        var islocked = await locker.TryLockAsync(cmd.TicketId, cmd.LockBy, timeout, ct);
        if (!islocked)
        {
            throw new InvalidOperationException("Ticket is being locked by another connection.");
        }

        // 3. Load ticket aggregate and release transient gate key if ticket no longer exists.
        var ticket = await uow.Tickets.GetByIdAsync(cmd.TicketId, ct);
        if (ticket is null)
        {
            await locker.ReleaseAsync(cmd.TicketId, cmd.LockBy, ct);

            throw new InvalidOperationException($"Ticket with ID '{cmd.TicketId}' not found.");
        }

        var showTime = await uow.ShowTimes.GetByIdAsync(ticket.ShowTimeId, ct);
        if (showTime is null || showTime.Status == ShowTimeStatus.Cancelled)
        {
            await locker.ReleaseAsync(cmd.TicketId, cmd.LockBy, ct);
            throw new InvalidOperationException("Cannot lock tickets for a canceled showtime.");
        }

        // 4. Apply domain transition to Locking state and persist through unit of work.
        ticket.Lock(cmd.LockBy, lockExpiresAt);
        uow.Tickets.Update(ticket);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates ticket lock command payload.
/// </summary>
public class LockTicketValidator : AbstractValidator<LockTicketCommand>
{
    public LockTicketValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.LockBy)
            .NotEmpty().WithMessage("Lock owner is required.")
            .MaximumLength(MaxLengthConsts.SessionId);
    }
}
