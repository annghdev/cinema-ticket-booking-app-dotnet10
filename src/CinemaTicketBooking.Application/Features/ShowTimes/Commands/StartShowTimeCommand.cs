namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Starts an upcoming showtime.
/// </summary>
public class StartShowTimeCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles showtime start requests.
/// </summary>
public class StartShowTimeHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Starts the target showtime and persists changes.
    /// </summary>
    public async Task Handle(StartShowTimeCommand cmd, CancellationToken ct)
    {
        var showTime = await uow.ShowTimes.GetByIdAsync(cmd.Id, ct);
        if (showTime is null)
        {
            throw new InvalidOperationException($"ShowTime with ID '{cmd.Id}' not found.");
        }

        showTime.StartShowing();
        uow.ShowTimes.Update(showTime);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates start-showtime command payload.
/// </summary>
public class StartShowTimeValidator : AbstractValidator<StartShowTimeCommand>
{
    public StartShowTimeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ShowTime ID is required.");
    }
}
