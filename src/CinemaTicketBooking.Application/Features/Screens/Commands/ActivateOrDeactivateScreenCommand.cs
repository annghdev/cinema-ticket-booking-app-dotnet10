namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Activates an existing screen.
/// </summary>
public class ActivateScreenCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles screen activation requests.
/// </summary>
public class ActivateScreenHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Activates the target screen and persists changes.
    /// </summary>
    public async Task Handle(ActivateScreenCommand cmd, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdAsync(cmd.Id, ct);
        if (screen is null)
        {
            throw new InvalidOperationException($"Screen with ID '{cmd.Id}' not found.");
        }

        screen.Activate();
        uow.Screens.Update(screen);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Deactivates an existing screen.
/// </summary>
public class DeactivateScreenCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles screen deactivation requests.
/// </summary>
public class DeactivateScreenHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deactivates the target screen and persists changes.
    /// </summary>
    public async Task Handle(DeactivateScreenCommand cmd, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdAsync(cmd.Id, ct);
        if (screen is null)
        {
            throw new InvalidOperationException($"Screen with ID '{cmd.Id}' not found.");
        }

        screen.Deactivate();
        uow.Screens.Update(screen);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates activate-screen command payload.
/// </summary>
public class ActivateScreenValidator : AbstractValidator<ActivateScreenCommand>
{
    public ActivateScreenValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Screen ID is required.");
    }
}

/// <summary>
/// Validates deactivate-screen command payload.
/// </summary>
public class DeactivateScreenValidator : AbstractValidator<DeactivateScreenCommand>
{
    public DeactivateScreenValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Screen ID is required.");
    }
}
