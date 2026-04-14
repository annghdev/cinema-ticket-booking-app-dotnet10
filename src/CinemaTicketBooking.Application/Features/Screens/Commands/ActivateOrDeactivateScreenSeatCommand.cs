namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Activates a seat in a screen.
/// </summary>
public class ActivateScreenSeatCommand : ICommand
{
    public Guid ScreenId { get; set; }
    public Guid SeatId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles screen-seat activation requests.
/// </summary>
public class ActivateScreenSeatHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Activates the target seat in a screen and persists changes.
    /// </summary>
    public async Task Handle(ActivateScreenSeatCommand cmd, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdWithSeatsAsync(cmd.ScreenId, ct);
        if (screen is null)
        {
            throw new InvalidOperationException($"Screen with ID '{cmd.ScreenId}' not found.");
        }

        screen.ActivateSeat(cmd.SeatId);
        uow.Screens.Update(screen);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Deactivates a seat in a screen.
/// </summary>
public class DeactivateScreenSeatCommand : ICommand
{
    public Guid ScreenId { get; set; }
    public Guid SeatId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles screen-seat deactivation requests.
/// </summary>
public class DeactivateScreenSeatHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deactivates the target seat in a screen and persists changes.
    /// </summary>
    public async Task Handle(DeactivateScreenSeatCommand cmd, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdWithSeatsAsync(cmd.ScreenId, ct);
        if (screen is null)
        {
            throw new InvalidOperationException($"Screen with ID '{cmd.ScreenId}' not found.");
        }

        screen.DeactivateSeat(cmd.SeatId);
        uow.Screens.Update(screen);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates activate-seat command payload.
/// </summary>
public class ActivateScreenSeatValidator : AbstractValidator<ActivateScreenSeatCommand>
{
    public ActivateScreenSeatValidator()
    {
        RuleFor(x => x.ScreenId)
            .NotEmpty().WithMessage("Screen ID is required.");

        RuleFor(x => x.SeatId)
            .NotEmpty().WithMessage("Seat ID is required.");
    }
}

/// <summary>
/// Validates deactivate-seat command payload.
/// </summary>
public class DeactivateScreenSeatValidator : AbstractValidator<DeactivateScreenSeatCommand>
{
    public DeactivateScreenSeatValidator()
    {
        RuleFor(x => x.ScreenId)
            .NotEmpty().WithMessage("Screen ID is required.");

        RuleFor(x => x.SeatId)
            .NotEmpty().WithMessage("Seat ID is required.");
    }
}
