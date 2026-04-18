using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fakes;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.MessagingTests;

[Collection(DatabaseCollection.Name)]
public class TicketRealtimeEventHandlersTests(PostgresContainerFixture databaseFixture) : IAsyncLifetime
{
    private IHost? _messageHost;

    public async Task InitializeAsync()
    {
        _messageHost = await ApplicationMessageHostFactory.StartAsync(databaseFixture.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_messageHost is not null)
        {
            await _messageHost.StopAsync();
            _messageHost.Dispose();
        }
    }

    [Fact]
    public async Task LockAndStartPayment_Should_PublishRealtimeStatusDeltas()
    {
        await databaseFixture.ResetDatabaseAsync();
        var (ticketId, showTimeId) = await SeedTicketGraphAsync();
        var publisher = GetRealtimePublisher();
        publisher.Reset();

        var bus = GetBus();
        await bus.InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-realtime",
            CorrelationId = "it-realtime-lock"
        });
        await bus.InvokeAsync(new StartTicketPaymentCommand
        {
            TicketId = ticketId,
            BookingId = Guid.CreateVersion7(),
            StartBy = "session-realtime",
            CorrelationId = "it-realtime-start-payment"
        });

        await WaitUntilAsync(() => publisher.Events.Count >= 2, TimeSpan.FromSeconds(3));

        publisher.Events.Should().Contain(x => x.ShowTimeId == showTimeId && x.TicketId == ticketId && x.Status == TicketStatus.Locking);
        publisher.Events.Should().Contain(x => x.ShowTimeId == showTimeId && x.TicketId == ticketId && x.Status == TicketStatus.PendingPayment);
    }

    [Fact]
    public async Task LockTicket_Should_PublishDelta_ForExactShowtimeOnly()
    {
        await databaseFixture.ResetDatabaseAsync();
        var (ticketId, showTimeId) = await SeedTicketGraphAsync();
        var (_, otherShowTimeId) = await SeedTicketGraphAsync();
        var publisher = GetRealtimePublisher();
        publisher.Reset();

        var bus = GetBus();
        await bus.InvokeAsync(new LockTicketCommand
        {
            TicketId = ticketId,
            LockBy = "session-realtime-group-check",
            CorrelationId = "it-realtime-group-check"
        });

        await WaitUntilAsync(() => publisher.Events.Any(x => x.Status == TicketStatus.Locking), TimeSpan.FromSeconds(3));

        publisher.Events.Should().Contain(x => x.ShowTimeId == showTimeId && x.TicketId == ticketId && x.Status == TicketStatus.Locking);
        publisher.Events.Should().NotContain(x => x.ShowTimeId == otherShowTimeId && x.TicketId == ticketId);
    }

    private IMessageBus GetBus()
    {
        return _messageHost?.Services.GetRequiredService<IMessageBus>()
            ?? throw new InvalidOperationException("Message host is not initialized.");
    }

    private InMemoryTicketRealtimePublisher GetRealtimePublisher()
    {
        return _messageHost?.Services.GetRequiredService<InMemoryTicketRealtimePublisher>()
            ?? throw new InvalidOperationException("Realtime publisher probe is not initialized.");
    }

    private async Task<(Guid TicketId, Guid ShowTimeId)> SeedTicketGraphAsync()
    {
        await using var db = databaseFixture.CreateDbContext();

        var suffix = Guid.CreateVersion7().ToString("N")[..6];
        var cinema = IntegrationEntityBuilder.Cinema($"Realtime Cinema {suffix}");
        var movie = IntegrationEntityBuilder.Movie($"Realtime Movie {suffix}");
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"RT-{suffix}", "[[1,0,0]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id, $"RT-{suffix}-A1");

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Tickets.Add(ticket);

        await db.SaveChangesAsync();
        return (ticket.Id, showTime.Id);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - startedAt < timeout)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(120);
        }

        throw new TimeoutException($"Condition was not met within {timeout.TotalSeconds} seconds.");
    }
}
