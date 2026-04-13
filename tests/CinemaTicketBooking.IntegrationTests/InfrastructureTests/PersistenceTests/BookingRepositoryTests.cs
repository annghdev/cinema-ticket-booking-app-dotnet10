using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

public sealed class BookingRepositoryTests(PostgresContainerFixture databaseFixture)
    : RepositoryTestBase(databaseFixture)
{
    [Fact]
    public async Task LoadFullAsync_Should_ReturnFullAggregate_When_BookingExists()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new BookingRepository(db);

        var cinema = IntegrationEntityBuilder.Cinema();
        var movie = IntegrationEntityBuilder.Movie();
        var screen = IntegrationEntityBuilder.Screen(cinema.Id);
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var customer = IntegrationEntityBuilder.Customer();
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id, "T-100", Domain.TicketStatus.Locking);
        var concession = IntegrationEntityBuilder.Concession();
        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id);
        booking.Tickets.Add(new Domain.BookingTicket
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            TicketId = ticket.Id,
            Ticket = ticket
        });
        booking.Concessions.Add(new Domain.BookingConcession
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            ConcessionId = concession.Id,
            Concession = concession,
            Quantity = 1
        });

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Customers.Add(customer);
        db.Tickets.Add(ticket);
        db.Concessions.Add(concession);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        var loaded = await repository.LoadFullAsync(booking.Id);

        loaded.Should().NotBeNull();
        loaded!.ShowTime.Should().NotBeNull();
        loaded.Customer.Should().NotBeNull();
        loaded.Tickets.Should().ContainSingle();
        loaded.Tickets.Single().Ticket.Should().NotBeNull();
        loaded.Concessions.Should().ContainSingle();
        loaded.Concessions.Single().Concession.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadFullAsync_Should_ReturnNull_When_BookingDoesNotExist()
    {
        await DatabaseFixture.ResetDatabaseAsync();
        await using var db = CreateDbContext();
        var repository = new BookingRepository(db);

        var loaded = await repository.LoadFullAsync(Guid.CreateVersion7());

        loaded.Should().BeNull();
    }
}
