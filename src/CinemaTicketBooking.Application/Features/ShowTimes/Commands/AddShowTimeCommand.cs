using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Adds a new showtime.
/// </summary>
public class AddShowTimeCommand : ICommand
{
    public Guid MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public string CorrelationId {  get; set; } = string.Empty;
}

/// <summary>
/// Handles add-showtime requests.
/// </summary>
public class AddShowTimeHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Creates a showtime and persists changes.
    /// </summary>
    public async Task<Guid> Handle(AddShowTimeCommand command, CancellationToken ct)
    {
        // 1. Load Movie and Screen
        var movie = await LoadMovie(command.MovieId, ct);
        var screen = await LoadScreen(command.ScreenId, ct);

        // 2. Call domain service to create ShowTime
        var domainService = new ShowTimeSchedulingService(uow.ShowTimes, uow.PricingPolicies);
        var showTime = await domainService.ScheduleAsync(movie, screen, command.StartAt, ct);

        // 3. Add to repository and save
        uow.ShowTimes.Add(showTime);
        await uow.CommitAsync(ct);
        return showTime.Id;
    }

    private async Task<Movie> LoadMovie(Guid movieId, CancellationToken ct)
    {
        var movie = await uow.Movies.GetByIdAsync(movieId, ct);
        if (movie is null)
            throw new InvalidOperationException($"Movie with ID '{movieId}' not found.");
        return movie;
    }

    private async Task<Screen> LoadScreen(Guid screenId, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdWithSeatsAsync(screenId, ct);
        if (screen is null)
            throw new InvalidOperationException($"Screen with ID '{screenId}' not found.");
        return screen;
    }
}

/// <summary>
/// Validates add-showtime command payload.
/// </summary>
public class AddShowTimeValidator : AbstractValidator<AddShowTimeCommand>
{
    public AddShowTimeValidator()
    {
        RuleFor(x => x.MovieId)
            .NotEmpty().WithMessage("Movie ID is required.");

        RuleFor(x => x.ScreenId)
            .NotEmpty().WithMessage("Screen ID is required.");

        RuleFor(x => x.StartAt)
            .Must(x => x > DateTimeOffset.UtcNow)
            .WithMessage("StartAt must be in the future.");
    }
}
