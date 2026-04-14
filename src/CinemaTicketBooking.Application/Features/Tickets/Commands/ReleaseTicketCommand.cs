namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Releases a ticket for the current lock owner.
/// </summary>
public class ReleaseTicketCommand : ICommand
{
    public Guid TicketId { get; set; }
    public string ReleaseBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles ticket release requests from lock owner.
/// </summary>
public class ReleaseTicketHandler(IUnitOfWork uow, ITicketLocker locker)
{
    /// <summary>
    /// Releases ticket state and best-effort cleans transient lock key.
    /// </summary>
    public async Task Handle(ReleaseTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await uow.Tickets.GetByIdAsync(cmd.TicketId, ct);
        if (ticket is null)
        {
            throw new InvalidOperationException($"Ticket with ID '{cmd.TicketId}' not found.");
        }

        ticket.Release(cmd.ReleaseBy);
        uow.Tickets.Update(ticket);
        await uow.CommitAsync(ct);

        await locker.ReleaseAsync(cmd.TicketId, cmd.ReleaseBy, ct);
    }
}

/// <summary>
/// Validates ticket release command payload.
/// </summary>
public class ReleaseTicketValidator : AbstractValidator<ReleaseTicketCommand>
{
    public ReleaseTicketValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.ReleaseBy)
            .NotEmpty().WithMessage("Release owner is required.")
            .MaximumLength(MaxLengthConsts.SessionId);
    }
}
