using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

public sealed class BaseRepositoryContractTests(PostgresContainerFixture databaseFixture)
    : RepositoryTestBase(databaseFixture)
{
    [Fact]
    public async Task MovieRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new MovieRepository(db);

        var movie = IntegrationEntityBuilder.Movie(name: "Before");
        repository.Add(movie);
        await db.SaveChangesAsync();

        (await repository.ExistsAsync(x => x.Id == movie.Id)).Should().BeTrue();
        (await repository.GetByIdAsync(movie.Id)).Should().NotBeNull();

        movie.Name = "After";
        repository.Update(movie);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(movie.Id))!.Name.Should().Be("After");

        repository.Delete(movie);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(movie.Id)).Should().BeNull();
    }

    [Fact]
    public async Task CinemaRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new CinemaRepository(db);

        var cinema = IntegrationEntityBuilder.Cinema();
        repository.Add(cinema);
        await db.SaveChangesAsync();

        (await repository.GetAllAsync()).Should().ContainSingle(x => x.Id == cinema.Id);

        cinema.Name = "Cinema B";
        repository.Update(cinema);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(cinema.Id))!.Name.Should().Be("Cinema B");

        repository.Delete(cinema);
        await db.SaveChangesAsync();
        (await repository.ExistsAsync(x => x.Id == cinema.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ConcessionRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new ConcessionRepository(db);

        var concession = IntegrationEntityBuilder.Concession();
        repository.Add(concession);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(concession.Id)).Should().NotBeNull();

        concession.Price = 75_000m;
        repository.Update(concession);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(concession.Id))!.Price.Should().Be(75_000m);

        repository.Delete(concession);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(concession.Id)).Should().BeNull();
    }

    [Fact]
    public async Task CustomerRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new CustomerRepository(db);

        var customer = IntegrationEntityBuilder.Customer("session-a");
        repository.Add(customer);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(customer.Id)).Should().NotBeNull();

        customer.Name = "Registered Customer";
        repository.Update(customer);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(customer.Id))!.Name.Should().Be("Registered Customer");

        repository.Delete(customer);
        await db.SaveChangesAsync();
        (await repository.ExistsAsync(x => x.Id == customer.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task PricingPolicyRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new PricingPolicyRepository(db);

        var policy = IntegrationEntityBuilder.PricingPolicy(cinemaId: null);
        repository.Add(policy);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(policy.Id)).Should().NotBeNull();

        policy.BasePrice = 120_000m;
        repository.Update(policy);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(policy.Id))!.BasePrice.Should().Be(120_000m);

        repository.Delete(policy);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(policy.Id)).Should().BeNull();
    }

    [Fact]
    public async Task ScreenRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new ScreenRepository(db);

        var cinema = IntegrationEntityBuilder.Cinema();
        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();

        var screen = IntegrationEntityBuilder.Screen(cinema.Id);
        repository.Add(screen);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(screen.Id)).Should().NotBeNull();

        screen.Code = "S2";
        repository.Update(screen);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(screen.Id))!.Code.Should().Be("S2");

        repository.Delete(screen);
        await db.SaveChangesAsync();
        (await repository.ExistsAsync(x => x.Id == screen.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ShowTimeRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new ShowTimeRepository(db);

        var movie = IntegrationEntityBuilder.Movie();
        var cinema = IntegrationEntityBuilder.Cinema();
        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var screen = IntegrationEntityBuilder.Screen(cinema.Id);
        db.Screens.Add(screen);
        await db.SaveChangesAsync();

        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        repository.Add(showTime);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(showTime.Id)).Should().NotBeNull();

        showTime.Status = ShowTimeStatus.Showing;
        repository.Update(showTime);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(showTime.Id))!.Status.Should().Be(ShowTimeStatus.Showing);

        repository.Delete(showTime);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(showTime.Id)).Should().BeNull();
    }

    [Fact]
    public async Task TicketRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new TicketRepository(db);
        var (showTime, _, _) = await SeedShowTimeGraphAsync(db);

        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id, "T-001");
        repository.Add(ticket);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(ticket.Id)).Should().NotBeNull();

        ticket.Price = 150_000m;
        repository.Update(ticket);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(ticket.Id))!.Price.Should().Be(150_000m);

        repository.Delete(ticket);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(ticket.Id)).Should().BeNull();
    }

    [Fact]
    public async Task BookingRepository_Should_Support_BasicCrudContract()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new BookingRepository(db);
        var (showTime, _, _) = await SeedShowTimeGraphAsync(db);

        var customer = IntegrationEntityBuilder.Customer();
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id);
        repository.Add(booking);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(booking.Id)).Should().NotBeNull();

        booking.CustomerName = "Updated Booker";
        repository.Update(booking);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(booking.Id))!.CustomerName.Should().Be("Updated Booker");

        repository.Delete(booking);
        await db.SaveChangesAsync();
        (await repository.GetByIdAsync(booking.Id)).Should().BeNull();
    }

    private static async Task<(ShowTime ShowTime, Movie Movie, Screen Screen)> SeedShowTimeGraphAsync(AppDbContext db)
    {
        var cinema = IntegrationEntityBuilder.Cinema();
        var movie = IntegrationEntityBuilder.Movie();
        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var screen = IntegrationEntityBuilder.Screen(cinema.Id);
        db.Screens.Add(screen);
        await db.SaveChangesAsync();

        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        db.ShowTimes.Add(showTime);
        await db.SaveChangesAsync();

        return (showTime, movie, screen);
    }
}
