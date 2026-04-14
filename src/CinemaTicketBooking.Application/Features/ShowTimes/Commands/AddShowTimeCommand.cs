using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Features;

public class AddShowTimeCommand : ICommand
{
    public Guid MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public ShowTimeStatus Status { get; set; }
    public string CorrelationId {  get; set; } = string.Empty;
}

public class AddShowTimeHandler(IUnitOfWork uow)
{
    public async Task Handle(AddShowTimeCommand command, CancellationToken ct)
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
        var screen = await uow.Screens.GetByIdAsync(screenId, ct);
        if (screen is null)
            throw new InvalidOperationException($"Screen with ID '{screenId}' not found.");
        return screen;
    }
}
