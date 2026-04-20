using System.Net;
using System.Net.Http.Json;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Features.Bookings.Commands;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketBooking.IntegrationTests.ApiTests;

[Collection(nameof(AuthAlbaCollection))]
public sealed class PaymentEndpointsTests(AuthAlbaFixture fixture)
{
    [Fact]
    public async Task FakeCallback_Should_Confirm_Payment_And_Update_Booking_When_Valid()
    {
        // 1. Arrange
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cinema = IntegrationEntityBuilder.Cinema($"API Payment Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"API Payment Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"API-SCR-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);

        var customer = IntegrationEntityBuilder.Customer($"session-{Guid.CreateVersion7():N}");

        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id, "Fake Callback Tester");
        booking.OriginAmount = 100_000m;
        booking.FinalAmount = 100_000m;
        booking.Status = BookingStatus.Pending;

        var gatewayTxId = $"TEST-GW-{Guid.CreateVersion7():N}";
        var pTx = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = PaymentMethod.None,
            Amount = 100_000m,
            GatewayTransactionId = gatewayTxId,
            PaymentUrl = "https://fake.url",
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Customers.Add(customer);
        db.Bookings.Add(booking);
        db.PaymentTransactions.Add(pTx);
        await db.SaveChangesAsync();

        // 2. Act
        var client = fixture.CreateClient();
        
        // Pass response code "00" (success pattern mapped in NoPaymentGatewayService) 
        var url = $"/api/payments/fake-callback?bookingId={booking.Id}&transactionId={gatewayTxId}&vnp_ResponseCode=00";
        var response = await client.GetAsync(url);
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var resultBody = await response.Content.ReadFromJsonAsync<VerifyPaymentResponse>();
        resultBody.Should().NotBeNull();
        resultBody!.IsSuccess.Should().BeTrue();
        resultBody.BookingId.Should().Be(booking.Id);
        resultBody.CheckinQrCode.Should().NotBeNullOrEmpty();
        resultBody.Status.Should().Be("confirmed");
        
        // 4. Verify Database State
        var db2 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var updatedTx = await db2.PaymentTransactions.AsNoTracking().SingleAsync(x => x.Id == pTx.Id);
        updatedTx.Status.Should().Be(PaymentTransactionStatus.Success);
        
        var updatedBooking = await db2.Bookings.AsNoTracking().SingleAsync(x => x.Id == booking.Id);
        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
        updatedBooking.QrCode.Should().NotBeNull();
    }

    [Fact]
    public async Task FakeCallback_Should_Handle_Payment_Failure_Properly()
    {
        // 1. Arrange
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cinema = IntegrationEntityBuilder.Cinema($"API Payment Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"API Payment Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"API-SCR-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);

        var customerSessionId = $"session-{Guid.CreateVersion7():N}";
        var customer = IntegrationEntityBuilder.Customer(customerSessionId);

        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id, "Fake Callback Fail Tester");
        booking.OriginAmount = 100_000m;
        booking.FinalAmount = 100_000m;
        booking.Status = BookingStatus.Pending;

        var gatewayTxId = $"TEST-GW-FAIL-{Guid.CreateVersion7():N}";
        var pTx = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = PaymentMethod.None,
            Amount = 100_000m,
            GatewayTransactionId = gatewayTxId,
            PaymentUrl = "https://fake.url",
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Customers.Add(customer);
        db.Bookings.Add(booking);
        db.PaymentTransactions.Add(pTx);
        await db.SaveChangesAsync();

        // 2. Act
        var client = fixture.CreateClient();
        
        // Pass response code "99" (failure)
        var url = $"/api/payments/fake-callback?bookingId={booking.Id}&transactionId={gatewayTxId}&vnp_ResponseCode=99&vnp_SecureHash=invalid";
        var response = await client.GetAsync(url);
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var resultBody = await response.Content.ReadFromJsonAsync<VerifyPaymentResponse>();
        resultBody.Should().NotBeNull();
        resultBody!.IsSuccess.Should().BeFalse();
        resultBody.CanRetry.Should().BeTrue();
        resultBody.Status.Should().Be("payment_failed");
        
        // 4. Verify Database State
        var db2 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var updatedTx = await db2.PaymentTransactions.AsNoTracking().SingleAsync(x => x.Id == pTx.Id);
        updatedTx.Status.Should().Be(PaymentTransactionStatus.Failed);
        
        var updatedBooking = await db2.Bookings.AsNoTracking().SingleAsync(x => x.Id == booking.Id);
        updatedBooking.Status.Should().Be(BookingStatus.Pending); // Should still be Pending
    }

    [Fact]
    public async Task RetryPayment_Should_Create_New_Transaction()
    {
        // 1. Arrange
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cinema = IntegrationEntityBuilder.Cinema($"API Payment Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"API Payment Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"API-SCR-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        
        var ticket = IntegrationEntityBuilder.Ticket(showTime.Id);
        ticket.Status = TicketStatus.PendingPayment;
        ticket.ExtendPaymentHold(DateTimeOffset.UtcNow.AddMinutes(15));
        
        var customerSessionId = $"session-{Guid.CreateVersion7():N}";
        var customer = IntegrationEntityBuilder.Customer(customerSessionId);

        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id, "Retry Payment Tester");
        booking.OriginAmount = 100_000m;
        booking.FinalAmount = 100_000m;
        booking.Status = BookingStatus.Pending;

        // Add Ticket to Booking
        booking.Tickets.Add(new BookingTicket 
        { 
            Id = Guid.CreateVersion7(), 
            BookingId = booking.Id, 
            TicketId = ticket.Id,
            Ticket = ticket
        });

        // Add a previous failed transaction
        var failedTxId = $"TEST-GW-FAIL-{Guid.CreateVersion7():N}";
        var failedTx = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = PaymentMethod.None,
            Amount = 100_000m,
            GatewayTransactionId = failedTxId,
            PaymentUrl = "https://fake.url/fail",
            Status = PaymentTransactionStatus.Failed,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Tickets.Add(ticket);
        db.Customers.Add(customer);
        db.Bookings.Add(booking);
        db.PaymentTransactions.Add(failedTx);
        await db.SaveChangesAsync();

        // 2. Act
        var client = fixture.CreateClient();
        
        var requestData = new CinemaTicketBooking.WebServer.ApiEndpoints.RetryPaymentRequest(
            customerSessionId, "None", "https://return.url", "127.0.0.1"
        );
        
        var response = await client.PostAsJsonAsync($"/api/bookings/{booking.Id}/retry-payment", requestData);
        
        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var resultBody = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();
        resultBody.Should().NotBeNull();
        resultBody!.BookingId.Should().Be(booking.Id);
        resultBody.PaymentStatus.Should().Be("pending_payment");
        resultBody.PaymentUrl.Should().NotBeNullOrEmpty();
        
        // 4. Verify Database State
        var db2 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var transactions = await db2.PaymentTransactions.AsNoTracking().Where(x => x.BookingId == booking.Id).ToListAsync();
        transactions.Count.Should().Be(2); // The failed one and the new pending one
        
        var pendingTx = transactions.Single(x => x.Status == PaymentTransactionStatus.Pending);
        pendingTx.Method.Should().Be(PaymentMethod.None);
        pendingTx.Amount.Should().Be(100_000m);
    }
}
