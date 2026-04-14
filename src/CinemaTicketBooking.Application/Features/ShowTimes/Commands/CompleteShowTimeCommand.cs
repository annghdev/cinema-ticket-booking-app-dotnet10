namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Completes a showing showtime.
/// </summary>
public class CompleteShowTimeCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles showtime completion requests.
/// </summary>
public class CompleteShowTimeHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Completes the target showtime and persists changes.
    /// </summary>
    public async Task Handle(CompleteShowTimeCommand cmd, CancellationToken ct)
    {
        var showTime = await uow.ShowTimes.GetByIdAsync(cmd.Id, ct);
        if (showTime is null)
        {
            throw new InvalidOperationException($"ShowTime with ID '{cmd.Id}' not found.");
        }

        showTime.Complete();
        uow.ShowTimes.Update(showTime);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates complete-showtime command payload.
/// </summary>
public class CompleteShowTimeValidator : AbstractValidator<CompleteShowTimeCommand>
{
    public CompleteShowTimeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ShowTime ID is required.");
    }
}
