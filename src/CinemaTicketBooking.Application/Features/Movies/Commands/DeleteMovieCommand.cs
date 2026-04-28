namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Deletes an existing movie.
/// </summary>
public class DeleteMovieCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles movie deletion requests.
/// </summary>
public class DeleteMovieCommandHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deletes the target movie and persists changes.
    /// </summary>
    public async Task Handle(DeleteMovieCommand command, CancellationToken ct)
    {
        var movie = await uow.Movies.GetByIdAsync(command.Id, ct);
        if (movie is null)
        {
            throw new InvalidOperationException($"Movie with ID '{command.Id}' not found.");
        }

        var hasShowTimes = await uow.ShowTimes.ExistsAsync(st => st.MovieId == command.Id
            && (st.Status == ShowTimeStatus.Upcoming || st.Status == ShowTimeStatus.Showing), ct);
        if (hasShowTimes)
        {
            throw new InvalidOperationException($"Cannot delete Movie with ID '{command.Id}' because it currently has active showtimes.");
        }

        movie.MarkAsDeleted();
        uow.Movies.Delete(movie);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates delete command payload.
/// </summary>
public class DeleteMovieValidator : AbstractValidator<DeleteMovieCommand>
{
    public DeleteMovieValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Movie ID is required.");
    }
}
