using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

public sealed class ShowTimeFeatureTests(PostgresContainerFixture databaseFixture)
    : ApplicationFeatureTestBase(databaseFixture)
{
    [Fact]
    public async Task AddShowTime_Should_CreateTickets_FromActiveSeats_WithCorrectPricing()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: true, includePricingPolicies: true);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(4),
            CorrelationId = "it-showtime-create"
        });

        await using var db = CreateDbContext();
        var saved = await db.ShowTimes
            .Include(x => x.Tickets)
            .SingleAsync(x => x.Id == showTimeId);

        saved.Status.Should().Be(ShowTimeStatus.Upcoming);
        saved.Tickets.Should().HaveCount(seed.ActiveSeatCount);
        saved.Tickets.Should().OnlyContain(x => x.Status == TicketStatus.Available);
        saved.Tickets.Count(x => x.Description.EndsWith($"{SeatType.Regular}")).Should().Be(1);
        saved.Tickets.Count(x => x.Description.EndsWith($"{SeatType.VIP}")).Should().Be(2);
        saved.Tickets.Where(x => x.Description.EndsWith($"{SeatType.Regular}"))
            .Should().OnlyContain(x => x.Price == 90_000m);
        saved.Tickets.Where(x => x.Description.EndsWith($"{SeatType.VIP}"))
            .Should().OnlyContain(x => x.Price == 140_000m);
    }

    [Fact]
    public async Task AddShowTime_Should_Throw_When_ScheduleConflictsOnSameScreen()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);
        var baseStart = DateTimeOffset.UtcNow.AddHours(6);

        await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = baseStart,
            CorrelationId = "it-showtime-first"
        });

        var act = async () => await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = baseStart.AddMinutes(60),
            CorrelationId = "it-showtime-conflict"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Schedule conflict:*");
    }

    [Fact]
    public async Task AddShowTime_Should_Allow_SameStartAt_OnDifferentScreens()
    {
        await ResetDatabaseAsync();

        var startAt = DateTimeOffset.UtcNow.AddHours(8);
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);
        var secondScreenId = await SeedSecondScreenAsync(seed.CinemaId);

        var firstShowTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = startAt,
            CorrelationId = "it-showtime-screen-a"
        });

        var secondShowTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = secondScreenId,
            StartAt = startAt,
            CorrelationId = "it-showtime-screen-b"
        });

        await using var db = CreateDbContext();
        var createdIds = await db.ShowTimes
            .Where(x => x.Id == firstShowTimeId || x.Id == secondShowTimeId)
            .Select(x => x.Id)
            .ToListAsync();

        createdIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddShowTime_Should_Throw_When_NoActivePricingPolicy()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: false);

        var act = async () => await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(3),
            CorrelationId = "it-showtime-no-policy"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No pricing policies found*");
    }

    [Fact]
    public async Task AddShowTime_Should_Throw_When_MovieNotFound()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);

        var act = async () => await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = Guid.CreateVersion7(),
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(4),
            CorrelationId = "it-showtime-movie-missing"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Movie with ID*not found.");
    }

    [Fact]
    public async Task StartAndCompleteShowTime_Should_TransitionToCompleted()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(5),
            CorrelationId = "it-showtime-create-transition"
        });

        await InvokeAsync(new StartShowTimeCommand { Id = showTimeId, CorrelationId = "it-showtime-start" });
        await InvokeAsync(new CompleteShowTimeCommand { Id = showTimeId, CorrelationId = "it-showtime-complete" });

        await using var db = CreateDbContext();
        var saved = await db.ShowTimes.SingleAsync(x => x.Id == showTimeId);
        saved.Status.Should().Be(ShowTimeStatus.Completed);
    }

    [Fact]
    public async Task CompleteShowTime_Should_Throw_When_ShowTimeIsNotShowing()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(7),
            CorrelationId = "it-showtime-create-upcoming"
        });

        var act = async () => await InvokeAsync(new CompleteShowTimeCommand
        {
            Id = showTimeId,
            CorrelationId = "it-showtime-invalid-complete"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only showing showtimes can be completed.");
    }

    [Fact]
    public async Task CancelShowTime_Should_TransitionToCancelled_ForUpcoming()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(9),
            CorrelationId = "it-showtime-create-cancel"
        });

        await InvokeAsync(new CancelShowTimeCommand { Id = showTimeId, CorrelationId = "it-showtime-cancel" });

        await using var db = CreateDbContext();
        var saved = await db.ShowTimes.SingleAsync(x => x.Id == showTimeId);
        saved.Status.Should().Be(ShowTimeStatus.Cancelled);
    }

    [Fact]
    public async Task CancelShowTime_Should_Throw_When_ShowTimeIsShowing()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = DateTimeOffset.UtcNow.AddHours(10),
            CorrelationId = "it-showtime-create-for-cancel-validation"
        });

        await InvokeAsync(new StartShowTimeCommand { Id = showTimeId, CorrelationId = "it-showtime-start-for-cancel-validation" });

        var act = async () => await InvokeAsync(new CancelShowTimeCommand
        {
            Id = showTimeId,
            CorrelationId = "it-showtime-invalid-cancel"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot cancel a showtime that is showing or already completed.");
    }

    [Fact]
    public async Task ShowTimeQueries_Should_ReturnDetailAndCollections()
    {
        await ResetDatabaseAsync();
        var seed = await SeedShowTimeDependenciesAsync(disableOneRegularSeat: false, includePricingPolicies: true);
        var startAt = DateTimeOffset.UtcNow.AddHours(11);

        var showTimeId = await InvokeAsync<Guid>(new AddShowTimeCommand
        {
            MovieId = seed.MovieId,
            ScreenId = seed.ScreenId,
            StartAt = startAt,
            CorrelationId = "it-showtime-create-for-queries"
        });

        var detail = await InvokeAsync<ShowTimeDetailDto?>(new GetShowTimeByIdQuery
        {
            Id = showTimeId,
            CorrelationId = "it-showtime-detail-query"
        });

        detail.Should().NotBeNull();
        detail!.Id.Should().Be(showTimeId);
        detail.TicketCount.Should().Be(seed.ActiveSeatCount);
        detail.AvailableTicketCount.Should().Be(seed.ActiveSeatCount);

        var list = await InvokeAsync<IReadOnlyList<ShowTimeDto>>(new GetShowTimesQuery
        {
            MovieId = seed.MovieId,
            CorrelationId = "it-showtime-list-query"
        });

        list.Should().ContainSingle(x => x.Id == showTimeId);

        var paged = await InvokeAsync<PagedResult<ShowTimeDto>>(new GetPagedShowTimesQuery
        {
            MovieId = seed.MovieId,
            PageNumber = 1,
            PageSize = 10,
            SortBy = "startat",
            SortDirection = "asc",
            CorrelationId = "it-showtime-paged-query"
        });

        paged.TotalItems.Should().BeGreaterThan(0);
        paged.Items.Should().Contain(x => x.Id == showTimeId);

        var dropdown = await InvokeAsync<IReadOnlyList<ShowTimeDropdownDto>>(new GetShowTimeDropdownQuery
        {
            MovieId = seed.MovieId,
            MaxItems = 10,
            CorrelationId = "it-showtime-dropdown-query"
        });

        dropdown.Should().Contain(x => x.Id == showTimeId);
    }

    private async Task<ShowTimeSeedResult> SeedShowTimeDependenciesAsync(
        bool disableOneRegularSeat,
        bool includePricingPolicies)
    {
        await using var db = CreateDbContext();

        var cinema = IntegrationEntityBuilder.Cinema("Showtime Cinema");
        var movie = IntegrationEntityBuilder.Movie("Showtime Movie", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, "SCR-1", "[[1,2,0],[1,2,0]]");

        if (disableOneRegularSeat)
        {
            var firstRegularSeat = screen.Seats.First(x => x.Type == SeatType.Regular);
            firstRegularSeat.IsActive = false;
        }

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);

        if (includePricingPolicies)
        {
            db.PricingPolicies.AddRange(
                new PricingPolicy
                {
                    Id = Guid.CreateVersion7(),
                    CinemaId = cinema.Id,
                    ScreenType = screen.Type,
                    SeatType = SeatType.Regular,
                    BasePrice = 90_000m,
                    ScreenCoefficient = 1m,
                    IsActive = true
                },
                new PricingPolicy
                {
                    Id = Guid.CreateVersion7(),
                    CinemaId = cinema.Id,
                    ScreenType = screen.Type,
                    SeatType = SeatType.VIP,
                    BasePrice = 100_000m,
                    ScreenCoefficient = 1.4m,
                    IsActive = true
                });
        }

        await db.SaveChangesAsync();

        return new ShowTimeSeedResult(
            cinema.Id,
            movie.Id,
            screen.Id,
            screen.Seats.Count(x => x.IsActive));
    }

    private async Task<Guid> SeedSecondScreenAsync(Guid cinemaId)
    {
        await using var db = CreateDbContext();
        var secondScreen = IntegrationEntityBuilder.Screen(cinemaId, "SCR-2", "[[1,2,0],[1,2,0]]");
        db.Screens.Add(secondScreen);
        await db.SaveChangesAsync();
        return secondScreen.Id;
    }

    private sealed record ShowTimeSeedResult(Guid CinemaId, Guid MovieId, Guid ScreenId, int ActiveSeatCount);
}
