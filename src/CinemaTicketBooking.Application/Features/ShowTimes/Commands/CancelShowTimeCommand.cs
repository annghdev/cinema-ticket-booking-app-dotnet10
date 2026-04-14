namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Cancels an upcoming showtime.
/// </summary>
public class CancelShowTimeCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles showtime cancellation requests.
/// </summary>
public class CancelShowTimeHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Cancels the target showtime and persists changes.
    /// </summary>
    public async Task Handle(CancelShowTimeCommand cmd, CancellationToken ct)
    {
        var showTime = await uow.ShowTimes.LoadFullAsync(cmd.Id, ct);
        if (showTime is null)
        {
            throw new InvalidOperationException($"ShowTime with ID '{cmd.Id}' not found.");
        }

        showTime.Cancel();
        uow.ShowTimes.Update(showTime);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates cancel-showtime command payload.
/// </summary>
public class CancelShowTimeValidator : AbstractValidator<CancelShowTimeCommand>
{
    public CancelShowTimeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ShowTime ID is required.");
    }
}
