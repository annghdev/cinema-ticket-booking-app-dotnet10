using System.Net;
using System.Net.Http.Json;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Features.Bookings.Commands;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.WebServer.ApiEndpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketBooking.IntegrationTests.ApiTests;

[Collection(nameof(AuthAlbaCollection))]
public sealed class CoreBookingFlowTests(AuthAlbaFixture fixture)
{
    [Fact]
    public async Task Full_Booking_Flow_From_Lock_To_Payment_Callback_Should_Succeed()
    {
        // 1. Arrange Data
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cinema = IntegrationEntityBuilder.Cinema($"E2E Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"E2E Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"E2E-SCR-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        
        // Add a ticket explicitly for the showtime
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id);
        ticket.Status = TicketStatus.Available;

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var client = fixture.CreateClient();
        var sessionId = $"e2e-session-{Guid.CreateVersion7():N}";

        // ==========================================
        // STEP 1: Lock Ticket
        // ==========================================
        var lockRequest = new LockTicketRequest(sessionId);
        var lockResponse = await client.PostAsJsonAsync($"/api/showtimes/{showTime.Id}/tickets/{ticket.Id}/lock", lockRequest);
        lockResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify ticket is locked in DB
        var dbAfterLock = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var lockedTicket = await dbAfterLock.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        lockedTicket.Status.Should().Be(TicketStatus.Locking);
        lockedTicket.LockingBy.Should().Be(sessionId);

        // ==========================================
        // STEP 2: Create Booking
        // ==========================================
        var createBookingPayload = new 
        {
            ShowTimeId = showTime.Id,
            CustomerSessionId = sessionId,
            CustomerName = "E2E Tester",
            CustomerEmail = "e2e@example.com",
            CustomerPhoneNumber = "0901234567",
            SelectedTicketIds = new[] { ticket.Id },
            Concessions = Array.Empty<object>(),
            PaymentMethod = "None",
            ReturnUrl = "http://localhost:3000/result",
            IpAddress = "127.0.0.1"
        };
        var bookingResponse = await client.PostAsJsonAsync("/api/bookings", createBookingPayload);
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var bookingResult = await bookingResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();
        bookingResult.Should().NotBeNull();
        bookingResult!.BookingId.Should().NotBeEmpty();
        bookingResult.PaymentStatus.Should().Be("pending_payment");
        bookingResult.PaymentTransactionId.Should().NotBeEmpty();

        var bookingId = bookingResult.BookingId;
        var pTxId = bookingResult.PaymentTransactionId;

        // Verify booking in DB
        var dbAfterBooking = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdBooking = await dbAfterBooking.Bookings.AsNoTracking().SingleAsync(b => b.Id == bookingId);
        createdBooking.Status.Should().Be(BookingStatus.Pending);

        var pendingTicket = await dbAfterBooking.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        pendingTicket.Status.Should().Be(TicketStatus.PendingPayment);

        var pTx = await dbAfterBooking.PaymentTransactions.AsNoTracking().SingleAsync(tx => tx.Id == pTxId);
        pTx.Status.Should().Be(PaymentTransactionStatus.Pending);
        pTx.GatewayTransactionId.Should().NotBeNullOrEmpty();

        var gatewayTxId = pTx.GatewayTransactionId;

        // ==========================================
        // STEP 3: Fake Payment Callback (Success)
        // ==========================================
        var callbackUrl = $"/api/payments/fake-callback?bookingId={bookingId}&transactionId={gatewayTxId}&vnp_ResponseCode=00";
        var callbackResponse = await client.GetAsync(callbackUrl);
        callbackResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify final DB state
        var dbFinal = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var finalBooking = await dbFinal.Bookings.AsNoTracking().SingleAsync(b => b.Id == bookingId);
        finalBooking.Status.Should().Be(BookingStatus.Confirmed);

        var finalTicket = await dbFinal.Tickets.AsNoTracking().SingleAsync(t => t.Id == ticket.Id);
        finalTicket.Status.Should().Be(TicketStatus.Sold);

        var finalTx = await dbFinal.PaymentTransactions.AsNoTracking().SingleAsync(tx => tx.Id == pTxId);
        finalTx.Status.Should().Be(PaymentTransactionStatus.Success);
    }
}
