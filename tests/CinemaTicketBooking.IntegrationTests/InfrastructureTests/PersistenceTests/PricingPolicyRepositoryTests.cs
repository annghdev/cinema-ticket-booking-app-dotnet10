using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

public sealed class PricingPolicyRepositoryTests(PostgresContainerFixture databaseFixture)
    : RepositoryTestBase(databaseFixture)
{
    [Fact]
    public async Task GetActivePoliciesAsync_Should_ReturnMatchingActivePolicies()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new PricingPolicyRepository(db);
        var cinema = IntegrationEntityBuilder.Cinema();
        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();

        var matchRegular = IntegrationEntityBuilder.PricingPolicy(cinema.Id, ScreenType.TwoD, SeatType.Regular, true);
        var matchVip = IntegrationEntityBuilder.PricingPolicy(cinema.Id, ScreenType.TwoD, SeatType.VIP, true);
        var inactive = IntegrationEntityBuilder.PricingPolicy(cinema.Id, ScreenType.TwoD, SeatType.Couple, false);
        var wrongScreen = IntegrationEntityBuilder.PricingPolicy(cinema.Id, ScreenType.IMAX, SeatType.Regular, true);
        db.PricingPolicies.AddRange(matchRegular, matchVip, inactive, wrongScreen);
        await db.SaveChangesAsync();

        var result = await repository.GetActivePoliciesAsync(cinema.Id, ScreenType.TwoD);

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain([matchRegular.Id, matchVip.Id]);
    }

    [Fact]
    public async Task GetActivePolicyAsync_Should_ReturnCinemaSpecificPolicy_When_BothSpecificAndDefaultExist()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new PricingPolicyRepository(db);
        var cinema = IntegrationEntityBuilder.Cinema();
        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();

        var defaultPolicy = IntegrationEntityBuilder.PricingPolicy(null, ScreenType.TwoD, SeatType.Regular, true);
        defaultPolicy.BasePrice = 80_000m;
        var cinemaPolicy = IntegrationEntityBuilder.PricingPolicy(cinema.Id, ScreenType.TwoD, SeatType.Regular, true);
        cinemaPolicy.BasePrice = 100_000m;
        db.PricingPolicies.AddRange(defaultPolicy, cinemaPolicy);
        await db.SaveChangesAsync();

        var result = await repository.GetActivePolicyAsync(cinema.Id, ScreenType.TwoD, SeatType.Regular);

        result.Should().NotBeNull();
        result!.Id.Should().Be(cinemaPolicy.Id);
    }

    [Fact]
    public async Task GetActivePolicyAsync_Should_FallbackToDefaultPolicy_When_CinemaSpecificPolicyDoesNotExist()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new PricingPolicyRepository(db);
        var cinema = IntegrationEntityBuilder.Cinema();
        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();

        var defaultPolicy = IntegrationEntityBuilder.PricingPolicy(null, ScreenType.TwoD, SeatType.VIP, true);
        db.PricingPolicies.Add(defaultPolicy);
        await db.SaveChangesAsync();

        var result = await repository.GetActivePolicyAsync(cinema.Id, ScreenType.TwoD, SeatType.VIP);

        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultPolicy.Id);
    }
}
