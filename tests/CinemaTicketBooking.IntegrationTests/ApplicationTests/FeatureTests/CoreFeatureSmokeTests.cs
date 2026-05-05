using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

public sealed class CoreFeatureSmokeTests(PostgresContainerFixture databaseFixture)
    : ApplicationFeatureTestBase(databaseFixture)
{
    [Fact]
    public async Task CinemaFeature_Should_CreateAndQueryById()
    {
        await ResetDatabaseAsync();

        var cinemaId = await InvokeAsync<Guid>(new CreateCinemaCommand
        {
            Name = "Smoke Cinema",
            ThumbnailUrl = "https://example.com/cinema-smoke.jpg",
            Geo = "10.762622,106.660172",
            Address = "Smoke Address",
            IsActive = true,
            CorrelationId = "it-cinema-create"
        });

        var cinema = await InvokeAsync<CinemaDto?>(new GetCinemaByIdQuery
        {
            Id = cinemaId,
            CorrelationId = "it-cinema-get-by-id"
        });

        cinema.Should().NotBeNull();
        cinema!.Id.Should().Be(cinemaId);
        cinema.Name.Should().Be("Smoke Cinema");
        cinema.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task MovieFeature_Should_CreateAndQueryById()
    {
        await ResetDatabaseAsync();

        var movieId = await InvokeAsync<Guid>(new CreateMovieCommand
        {
            Name = "Smoke Movie",
            Description = "Movie for integration smoke testing",
            ThumbnailUrl = "https://example.com/movie-smoke.jpg",
            Studio = "Smoke Studio",
            Director = "Smoke Director",
            OfficialTrailerUrl = "https://example.com/trailer-smoke",
            Duration = 110,
            Genre = MovieGenre.Action,
            Status = MovieStatus.NowShowing,
            CorrelationId = "it-movie-create"
        });

        var movie = await InvokeAsync<MovieDto?>(new GetMovieByIdQuery
        {
            Id = movieId,
            CorrelationId = "it-movie-get-by-id"
        });

        movie.Should().NotBeNull();
        movie!.Id.Should().Be(movieId);
        movie.Name.Should().Be("Smoke Movie");
        movie.Status.Should().Be(MovieStatus.NowShowing);
    }

    [Fact]
    public async Task ScreenFeature_Should_AddAndQueryById()
    {
        await ResetDatabaseAsync();
        var cinemaId = await SeedCinemaAsync();

        var screenId = await InvokeAsync<Guid>(new AddScreenCommand
        {
            CinemaId = cinemaId,
            Code = "SCN-SMOKE-1",
            RowOfSeats = 2,
            ColumnOfSeats = 3,
            TotalSeats = 4,
            SeatMap = "[[1,2,0],[1,2,0]]",
            SupportedFormats = [ScreenType.TwoD],
            CorrelationId = "it-screen-create"
        });

        var detail = await InvokeAsync<ScreenDetailDto?>(new GetScreenByIdQuery
        {
            Id = screenId,
            CorrelationId = "it-screen-get-by-id"
        });

        detail.Should().NotBeNull();
        detail!.Id.Should().Be(screenId);
        detail.Code.Should().Be("SCN-SMOKE-1");
        detail.Seats.Should().HaveCount(4);
    }

    [Fact]
    public async Task ConcessionFeature_Should_AddToggleAvailability_AndQueryById()
    {
        await ResetDatabaseAsync();

        var concessionId = await InvokeAsync<Guid>(new AddConcessionCommand
        {
            Name = "Smoke Popcorn",
            Price = 49_000m,
            ImageUrl = "https://example.com/popcorn-smoke.jpg",
            IsAvailable = true,
            CorrelationId = "it-concession-create"
        });

        await InvokeAsync(new SetConcessionUnavailableCommand
        {
            Id = concessionId,
            CorrelationId = "it-concession-set-unavailable"
        });

        var concession = await InvokeAsync<ConcessionDto?>(new GetConcessionByIdQuery
        {
            Id = concessionId,
            CorrelationId = "it-concession-get-by-id"
        });

        concession.Should().NotBeNull();
        concession!.Id.Should().Be(concessionId);
        concession.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task PricingPolicyFeature_Should_AddToggleActivation_AndQueryById()
    {
        await ResetDatabaseAsync();
        var cinemaId = await SeedCinemaAsync();

        var policyId = await InvokeAsync<Guid>(new AddPricingPolicyCommand
        {
            CinemaId = cinemaId,
            ScreenType = ScreenType.TwoD,
            SeatType = SeatType.Regular,
            BasePrice = 100_000m,
            ScreenCoefficient = 1.2m,
            IsActive = true,
            CorrelationId = "it-policy-create"
        });

        await InvokeAsync(new SetPricingPolicyInactiveCommand
        {
            Id = policyId,
            CorrelationId = "it-policy-set-inactive"
        });

        var policy = await InvokeAsync<PricingPolicyDto?>(new GetPricingPolicyByIdQuery
        {
            Id = policyId,
            CorrelationId = "it-policy-get-by-id"
        });

        policy.Should().NotBeNull();
        policy!.Id.Should().Be(policyId);
        policy.IsActive.Should().BeFalse();
        policy.FinalPrice.Should().Be(120_000m);
    }

    private async Task<Guid> SeedCinemaAsync()
    {
        await using var db = CreateDbContext();
        var cinema = IntegrationEntityBuilder.Cinema("Seeded Smoke Cinema");
        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();
        return cinema.Id;
    }
}
