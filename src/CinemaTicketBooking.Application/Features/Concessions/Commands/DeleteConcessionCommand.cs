namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Deletes an existing concession item.
/// </summary>
public class DeleteConcessionCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles concession deletion requests.
/// </summary>
public class DeleteConcessionHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deletes the target concession and persists changes.
    /// </summary>
    public async Task Handle(DeleteConcessionCommand cmd, CancellationToken ct)
    {
        var concession = await uow.Concessions.GetByIdAsync(cmd.Id, ct);
        if (concession is null)
        {
            throw new InvalidOperationException($"Concession with ID '{cmd.Id}' not found.");
        }

        concession.MarkAsDeleted();
        uow.Concessions.Delete(concession);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates delete command payload.
/// </summary>
public class DeleteConcessionValidator : AbstractValidator<DeleteConcessionCommand>
{
    public DeleteConcessionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Concession ID is required.");
    }
}
