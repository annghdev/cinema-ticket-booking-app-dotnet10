namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Deletes an existing cinema.
/// </summary>
public class DeleteCinemaCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles cinema deletion requests.
/// </summary>
public class DeleteCinemaHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deletes the target cinema and persists changes.
    /// </summary>
    public async Task Handle(DeleteCinemaCommand cmd, CancellationToken ct)
    {
        var cinema = await uow.Cinemas.GetByIdAsync(cmd.Id, ct);
        if (cinema is null)
        {
            throw new InvalidOperationException($"Cinema with ID '{cmd.Id}' not found.");
        }

        cinema.MarkAsDeleted();
        uow.Cinemas.Delete(cinema);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates delete command payload.
/// </summary>
public class DeleteCinemaValidator : AbstractValidator<DeleteCinemaCommand>
{
    public DeleteCinemaValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cinema ID is required.");
    }
}
