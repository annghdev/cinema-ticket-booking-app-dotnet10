namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Sets concession availability to available.
/// </summary>
public class SetConcessionAvailableCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles set-concession-available requests.
/// </summary>
public class SetConcessionAvailableHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Marks the target concession as available and persists changes.
    /// </summary>
    public async Task Handle(SetConcessionAvailableCommand cmd, CancellationToken ct)
    {
        var concession = await uow.Concessions.GetByIdAsync(cmd.Id, ct);
        if (concession is null)
        {
            throw new InvalidOperationException($"Concession with ID '{cmd.Id}' not found.");
        }

        concession.MarkAsAvailable();
        uow.Concessions.Update(concession);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Sets concession availability to unavailable.
/// </summary>
public class SetConcessionUnavailableCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles set-concession-unavailable requests.
/// </summary>
public class SetConcessionUnavailableHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Marks the target concession as unavailable and persists changes.
    /// </summary>
    public async Task Handle(SetConcessionUnavailableCommand cmd, CancellationToken ct)
    {
        var concession = await uow.Concessions.GetByIdAsync(cmd.Id, ct);
        if (concession is null)
        {
            throw new InvalidOperationException($"Concession with ID '{cmd.Id}' not found.");
        }

        concession.MarkAsUnavailable();
        uow.Concessions.Update(concession);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates set-concession-available command payload.
/// </summary>
public class SetConcessionAvailableValidator : AbstractValidator<SetConcessionAvailableCommand>
{
    public SetConcessionAvailableValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Concession ID is required.");
    }
}

/// <summary>
/// Validates set-concession-unavailable command payload.
/// </summary>
public class SetConcessionUnavailableValidator : AbstractValidator<SetConcessionUnavailableCommand>
{
    public SetConcessionUnavailableValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Concession ID is required.");
    }
}
