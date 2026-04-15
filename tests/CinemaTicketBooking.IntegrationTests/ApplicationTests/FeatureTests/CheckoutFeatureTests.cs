using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

public sealed class CheckoutFeatureTests(PostgresContainerFixture databaseFixture)
    : ApplicationFeatureTestBase(databaseFixture)
{
    [Fact]
    public async Task ValidatePreCheckoutSeatSelection_Should_Block_When_OrphanSeatIsCreated()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();

        var response = await InvokeAsync<PreCheckoutValidationResponse>(new ValidatePreCheckoutSeatSelectionCommand
        {
            ShowTimeId = seed.ShowTimeId,
            CustomerSessionId = seed.SessionId,
            SelectedTicketIds =
            [
                seed.TicketsBySeatCode["A1"],
                seed.TicketsBySeatCode["A2"],
                seed.TicketsBySeatCode["A4"],
                seed.TicketsBySeatCode["A5"]
            ],
            CorrelationId = "it-precheckout-orphan"
        });

        response.CanProceed.Should().BeFalse();
        response.Errors.Should().Contain(x => x.Type == "ORPHAN_SEAT");
    }

    [Fact]
    public async Task ValidatePreCheckoutSeatSelection_Should_Pass_When_ContiguousSelection()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();

        var response = await InvokeAsync<PreCheckoutValidationResponse>(new ValidatePreCheckoutSeatSelectionCommand
        {
            ShowTimeId = seed.ShowTimeId,
            CustomerSessionId = seed.SessionId,
            SelectedTicketIds =
            [
                seed.TicketsBySeatCode["A1"],
                seed.TicketsBySeatCode["A2"],
                seed.TicketsBySeatCode["A3"]
            ],
            CorrelationId = "it-precheckout-ok"
        });

        response.CanProceed.Should().BeTrue();
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateBookingAndProcessPayment_Should_UseSharedPaymentExpiry_ForAllSelectedTickets()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();
        var concessionId = await SeedConcessionAsync();

        var response = await InvokeAsync<CreateBookingAndProcessPaymentResponse>(new CreateBookingAndProcessPaymentCommand
        {
            ShowTimeId = seed.ShowTimeId,
            CustomerSessionId = seed.SessionId,
            CustomerName = "Guest Checkout",
            CustomerPhoneNumber = "0123456789",
            CustomerEmail = "guest.checkout@example.com",
            SelectedTicketIds =
            [
                seed.TicketsBySeatCode["A1"],
                seed.TicketsBySeatCode["A2"]
            ],
            Concessions = [new CheckoutConcessionSelection(concessionId, 2)],
            DiscountAmount = 10_000m,
            CorrelationId = "it-create-booking-process-payment"
        });

        await using var db = CreateDbContext();
        var booking = await db.Bookings
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Ticket)
            .SingleAsync(x => x.Id == response.BookingId);

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.Tickets.Should().HaveCount(2);
        booking.Tickets.Select(x => x.Ticket!.Status).Should().OnlyContain(x => x == TicketStatus.PendingPayment);
        booking.Tickets
            .Select(x => x.Ticket!.PaymentExpiresAt)
            .Distinct()
            .Should()
            .ContainSingle();
        booking.Tickets.First().Ticket!.PaymentExpiresAt
            .Should()
            .BeCloseTo(response.PaymentExpiresAt, precision: TimeSpan.FromMilliseconds(10));
        response.PaymentStatus.Should().Be("pending_payment");
        response.FinalAmount.Should().Be(booking.FinalAmount);
    }

    private async Task<(Guid ShowTimeId, string SessionId, Dictionary<string, Guid> TicketsBySeatCode)> SeedCheckoutGraphAsync()
    {
        await using var db = CreateDbContext();

        var cinema = IntegrationEntityBuilder.Cinema("Checkout Cinema");
        var movie = IntegrationEntityBuilder.Movie("Checkout Movie", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, "CHK-SCR-1", "[[1,1,1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);

        var sessionId = "checkout-session-1";
        var seatCodes = new[] { "A1", "A2", "A3", "A4", "A5" };
        var tickets = seatCodes.Select(seatCode => new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTime.Id,
            SeatId = screen.Seats.FirstOrDefault(x => x.Code == seatCode)?.Id,
            SeatCode = seatCode,
            Status = TicketStatus.Locking,
            LockingBy = sessionId,
            LockExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            Code = $"20260415-{screen.Code}-{seatCode}",
            Description = $"{seatCode} - Regular",
            Price = 100_000m
        }).ToList();

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Tickets.AddRange(tickets);

        await db.SaveChangesAsync();

        return (
            showTime.Id,
            sessionId,
            tickets.ToDictionary(
                x => x.SeatCode,
                x => x.Id));
    }

    private async Task<Guid> SeedConcessionAsync()
    {
        await using var db = CreateDbContext();
        var concession = IntegrationEntityBuilder.Concession("Big Combo", 80_000m);
        db.Concessions.Add(concession);
        await db.SaveChangesAsync();
        return concession.Id;
    }
}
