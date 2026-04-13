using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

public sealed class ShowTimeRepositoryTests(PostgresContainerFixture databaseFixture)
    : RepositoryTestBase(databaseFixture)
{
    [Fact]
    public async Task GetActiveByScreenAndDateRangeAsync_Should_FilterByStatusDateAndScreen()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new ShowTimeRepository(db);

        var cinema = IntegrationEntityBuilder.Cinema();
        var movie = IntegrationEntityBuilder.Movie();
        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var screenA = IntegrationEntityBuilder.Screen(cinema.Id, "S-A");
        var screenB = IntegrationEntityBuilder.Screen(cinema.Id, "S-B");
        db.Screens.AddRange(screenA, screenB);
        await db.SaveChangesAsync();

        var rangeStart = DateTimeOffset.UtcNow.AddHours(1);
        var rangeEnd = DateTimeOffset.UtcNow.AddHours(8);

        var inRangeOngoing = IntegrationEntityBuilder.ShowTime(movie.Id, screenA.Id, ShowTimeStatus.Ongoing);
        inRangeOngoing.StartAt = DateTimeOffset.UtcNow.AddHours(2);
        inRangeOngoing.EndAt = inRangeOngoing.StartAt.AddHours(2);

        var inRangeShowing = IntegrationEntityBuilder.ShowTime(movie.Id, screenA.Id, ShowTimeStatus.Showing);
        inRangeShowing.StartAt = DateTimeOffset.UtcNow.AddHours(4);
        inRangeShowing.EndAt = inRangeShowing.StartAt.AddHours(2);

        var cancelled = IntegrationEntityBuilder.ShowTime(movie.Id, screenA.Id, ShowTimeStatus.Cancelled);
        cancelled.StartAt = DateTimeOffset.UtcNow.AddHours(3);
        cancelled.EndAt = cancelled.StartAt.AddHours(2);

        var wrongScreen = IntegrationEntityBuilder.ShowTime(movie.Id, screenB.Id, ShowTimeStatus.Ongoing);
        wrongScreen.StartAt = DateTimeOffset.UtcNow.AddHours(3);
        wrongScreen.EndAt = wrongScreen.StartAt.AddHours(2);

        var outsideRange = IntegrationEntityBuilder.ShowTime(movie.Id, screenA.Id, ShowTimeStatus.Ongoing);
        outsideRange.StartAt = DateTimeOffset.UtcNow.AddHours(10);
        outsideRange.EndAt = outsideRange.StartAt.AddHours(2);

        db.ShowTimes.AddRange(inRangeOngoing, inRangeShowing, cancelled, wrongScreen, outsideRange);
        await db.SaveChangesAsync();

        var result = await repository.GetActiveByScreenAndDateRangeAsync(screenA.Id, rangeStart, rangeEnd);

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain([inRangeOngoing.Id, inRangeShowing.Id]);
    }

    [Fact]
    public async Task LoadFullAsync_Should_IncludeMovieScreenSeatsAndTickets()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new ShowTimeRepository(db);

        var cinema = IntegrationEntityBuilder.Cinema();
        var movie = IntegrationEntityBuilder.Movie();
        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var screen = IntegrationEntityBuilder.Screen(cinema.Id, "S1", "[[1,2,0],[1,1,0]]");
        db.Screens.Add(screen);
        await db.SaveChangesAsync();

        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id, "T-200");
        db.ShowTimes.Add(showTime);
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var loaded = await repository.LoadFullAsync(showTime.Id);

        loaded.Should().NotBeNull();
        loaded!.Movie.Should().NotBeNull();
        loaded.Screen.Should().NotBeNull();
        loaded.Screen!.Seats.Should().NotBeEmpty();
        loaded.Tickets.Should().ContainSingle();
    }
}
