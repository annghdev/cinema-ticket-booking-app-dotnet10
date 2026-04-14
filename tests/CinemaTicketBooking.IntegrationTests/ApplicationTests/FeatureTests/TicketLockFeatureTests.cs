using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

public sealed class TicketLockFeatureTests(PostgresContainerFixture databaseFixture)
    : ApplicationFeatureTestBase(databaseFixture)
{
    [Fact]
    public async Task LockTicket_Should_AllowOnlyOneWinner_When_RequestsRace()
    {
        await ResetDatabaseAsync();
        var ticketId = await SeedTicketGraphAsync();

        var first = TryLockAsync(ticketId, "session-a");
        var second = TryLockAsync(ticketId, "session-b");

        var results = await Task.WhenAll(first, second);
        results.Count(x => x).Should().Be(1);

        await using var db = CreateDbContext();
        var saved = await db.Tickets.SingleAsync(x => x.Id == ticketId);
        saved.Status.Should().Be(TicketStatus.Locking);
        saved.LockingBy.Should().BeOneOf("session-a", "session-b");
        saved.LockExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LockTicket_Should_Succeed_When_RedisGateIsUnavailable_UsingDbFallback()
    {
        await ResetDatabaseAsync();
        var ticketId = await SeedTicketGraphAsync();

        await InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-fallback",
            CorrelationId = "it-ticket-lock-fallback"
        });

        await using var db = CreateDbContext();
        var saved = await db.Tickets.SingleAsync(x => x.Id == ticketId);
        saved.Status.Should().Be(TicketStatus.Locking);
        saved.LockingBy.Should().Be("session-fallback");
        saved.LockExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TicketLocked_Should_AutoRelease_When_TimeoutExpires()
    {
        await ResetDatabaseAsync();
        var ticketId = await SeedTicketGraphAsync();

        await InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-timeout",
            CorrelationId = "it-ticket-lock-timeout"
        });

        await WaitUntilAsync(
            async () =>
            {
                await using var db = CreateDbContext();
                var ticket = await db.Tickets.SingleAsync(x => x.Id == ticketId);
                return ticket.Status == TicketStatus.Available;
            },
            TimeSpan.FromSeconds(8));

        await using var verifyDb = CreateDbContext();
        var released = await verifyDb.Tickets.SingleAsync(x => x.Id == ticketId);
        released.Status.Should().Be(TicketStatus.Available);
        released.LockingBy.Should().BeNull();
        released.LockExpiresAt.Should().BeNull();
        released.PaymentExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task RecoverStaleTicketLocks_Should_Release_ExpiredLockingTickets()
    {
        await ResetDatabaseAsync();
        var ticketId = await SeedTicketGraphAsync(
            status: TicketStatus.Locking,
            lockingBy: "session-stale",
            lockExpiresAt: DateTimeOffset.UtcNow.AddMinutes(-10));

        await InvokeAsync(new RecoverStaleTicketLocksCommand
        {
            CorrelationId = "it-ticket-recover-stale"
        });

        await WaitUntilAsync(
            async () =>
            {
                await using var db = CreateDbContext();
                var ticket = await db.Tickets.SingleAsync(x => x.Id == ticketId);
                return ticket.Status == TicketStatus.Available;
            },
            TimeSpan.FromSeconds(8));
    }

    [Fact]
    public async Task StartTicketPayment_Should_TransitionToPendingPayment_And_ClearLockingState()
    {
        await ResetDatabaseAsync();
        var bookingId = Guid.CreateVersion7();
        var ticketId = await SeedTicketGraphAsync();

        await InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-payment",
            CorrelationId = "it-ticket-lock-for-payment"
        });

        await InvokeAsync(new StartTicketPaymentCommand
        {
            TicketId = ticketId,
            BookingId = bookingId,
            StartBy = "session-payment",
            CorrelationId = "it-ticket-start-payment"
        });

        await using var db = CreateDbContext();
        var ticket = await db.Tickets.SingleAsync(x => x.Id == ticketId);
        ticket.Status.Should().Be(TicketStatus.PendingPayment);
        ticket.BookingId.Should().Be(bookingId);
        ticket.LockingBy.Should().BeNull();
        ticket.LockExpiresAt.Should().BeNull();
        ticket.PaymentExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PendingPayment_Should_AutoRelease_When_PaymentTimeoutExpires()
    {
        await ResetDatabaseAsync();
        var bookingId = Guid.CreateVersion7();
        var ticketId = await SeedTicketGraphAsync();

        await InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-payment-timeout",
            CorrelationId = "it-ticket-lock-for-payment-timeout"
        });

        await InvokeAsync(new StartTicketPaymentCommand
        {
            TicketId = ticketId,
            BookingId = bookingId,
            StartBy = "session-payment-timeout",
            CorrelationId = "it-ticket-start-payment-timeout"
        });

        await WaitUntilAsync(
            async () =>
            {
                await using var db = CreateDbContext();
                var ticket = await db.Tickets.SingleAsync(x => x.Id == ticketId);
                return ticket.Status == TicketStatus.Available;
            },
            TimeSpan.FromSeconds(10));

        await using var verifyDb = CreateDbContext();
        var released = await verifyDb.Tickets.SingleAsync(x => x.Id == ticketId);
        released.Status.Should().Be(TicketStatus.Available);
        released.BookingId.Should().BeNull();
        released.PaymentExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task RecoverStaleTicketLocks_Should_Release_ExpiredPendingPaymentTickets()
    {
        await ResetDatabaseAsync();
        var ticketId = await SeedTicketGraphAsync(
            status: TicketStatus.PendingPayment,
            bookingId: Guid.CreateVersion7(),
            paymentExpiresAt: DateTimeOffset.UtcNow.AddMinutes(-10));

        await InvokeAsync(new RecoverStaleTicketLocksCommand
        {
            CorrelationId = "it-ticket-recover-stale-payment"
        });

        await WaitUntilAsync(
            async () =>
            {
                await using var db = CreateDbContext();
                var ticket = await db.Tickets.SingleAsync(x => x.Id == ticketId);
                return ticket.Status == TicketStatus.Available;
            },
            TimeSpan.FromSeconds(8));
    }

    private async Task<Guid> SeedTicketGraphAsync(
        TicketStatus status = TicketStatus.Available,
        string? lockingBy = null,
        DateTimeOffset? lockExpiresAt = null,
        Guid? bookingId = null,
        DateTimeOffset? paymentExpiresAt = null)
    {
        await using var db = CreateDbContext();

        var cinema = IntegrationEntityBuilder.Cinema("Ticket Lock Cinema");
        var movie = IntegrationEntityBuilder.Movie("Ticket Lock Movie", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, "TKT-SCR-1", "[[1,0,0]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id, "TKT-LOCK-001", status);
        ticket.LockingBy = lockingBy;
        ticket.LockExpiresAt = lockExpiresAt;
        ticket.BookingId = bookingId;
        ticket.PaymentExpiresAt = paymentExpiresAt;

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Tickets.Add(ticket);

        await db.SaveChangesAsync();
        return ticket.Id;
    }

    private async Task<bool> TryLockAsync(Guid ticketId, string lockBy)
    {
        try
        {
            await InvokeAsync(new LockTicketCommand
            {
                TicketId = ticketId,
                LockBy = lockBy,
                CorrelationId = $"it-ticket-lock-race-{lockBy}"
            });
            return true;
        }
        catch (InvalidOperationException ex)
        {
            ex.Message.Should().Contain("locked");
            return false;
        }
    }

    private async Task WaitUntilAsync(Func<Task<bool>> predicate, TimeSpan timeout)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - startedAt < timeout)
        {
            if (await predicate())
            {
                return;
            }

            await Task.Delay(150);
        }

        throw new TimeoutException($"Condition was not met within {timeout.TotalSeconds} seconds.");
    }
}
