using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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

    [Fact]
    public async Task GetGateways_Should_Return_Icon_ForEachGateway()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/api/payments/gateways");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<PaymentGatewayOptionDto>>();
        payload.Should().NotBeNullOrEmpty();
        payload!.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Icon));
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_01_When_Order_NotFound()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/api/payments/vnpay-ipn?vnp_TxnRef=NOT-FOUND&vnp_Amount=10000000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("01");
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_04_When_Amount_Mismatch()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingVnpayTransactionAsync(db, amount: 100_000m);

        var client = fixture.CreateClient();
        var url = $"/api/payments/vnpay-ipn?vnp_TxnRef={transaction.GatewayTransactionId}&vnp_Amount=999";
        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("04");
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_02_When_Transaction_Already_Processed()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingVnpayTransactionAsync(db, amount: 100_000m);
        transaction.Status = PaymentTransactionStatus.Success;
        db.PaymentTransactions.Update(transaction);
        await db.SaveChangesAsync();

        var client = fixture.CreateClient();
        var url = $"/api/payments/vnpay-ipn?vnp_TxnRef={transaction.GatewayTransactionId}&vnp_Amount=10000000";
        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("02");
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_02_When_Booking_Was_Cancelled()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingVnpayTransactionAsync(db, amount: 100_000m);
        transaction.Status = PaymentTransactionStatus.Cancelled;
        db.PaymentTransactions.Update(transaction);
        await db.SaveChangesAsync();

        var client = fixture.CreateClient();
        var url = $"/api/payments/vnpay-ipn?vnp_TxnRef={transaction.GatewayTransactionId}&vnp_Amount=10000000";
        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("02");
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_97_When_Signature_Invalid()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingVnpayTransactionAsync(db, amount: 100_000m);

        var url = "/api/payments/vnpay-ipn"
                  + $"?vnp_TxnRef={transaction.GatewayTransactionId}"
                  + "&vnp_Amount=10000000"
                  + "&vnp_ResponseCode=00"
                  + "&vnp_TransactionStatus=00"
                  + "&vnp_SecureHash=invalid-signature";

        var client = fixture.CreateClient();
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("97");
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_00_And_Confirm_Booking_When_Valid()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingVnpayTransactionAsync(db, amount: 100_000m);

        var rawParams = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "10000000",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionStatus"] = "00",
            ["vnp_TxnRef"] = transaction.GatewayTransactionId
        };
        var signature = ComputeVnpaySignature(rawParams, "TEST_VNPAY_SECRET_KEY");
        var url = "/api/payments/vnpay-ipn"
                  + $"?vnp_TxnRef={transaction.GatewayTransactionId}"
                  + "&vnp_Amount=10000000"
                  + "&vnp_ResponseCode=00"
                  + "&vnp_TransactionStatus=00"
                  + $"&vnp_SecureHash={signature}";

        var client = fixture.CreateClient();
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("00");

        var db2 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var txAfter = await db2.PaymentTransactions.AsNoTracking().SingleAsync(x => x.Id == transaction.Id);
        txAfter.Status.Should().Be(PaymentTransactionStatus.Success);
        var bookingAfter = await db2.Bookings.AsNoTracking().SingleAsync(x => x.Id == transaction.BookingId);
        bookingAfter.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task VnpayIpn_Should_Return_99_When_TxnRef_Missing()
    {
        var client = fixture.CreateClient();
        var response = await client.GetAsync("/api/payments/vnpay-ipn?vnp_Amount=10000000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<VnpayIpnResponse>();
        payload.Should().NotBeNull();
        payload!.RspCode.Should().Be("99");
    }

    [Fact]
    public async Task MomoIpn_Should_Return_1002_When_Order_NotFound()
    {
        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", new
        {
            orderId = "NOT-FOUND",
            amount = "100000"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(1002);
    }

    [Fact]
    public async Task MomoIpn_Should_Return_1003_When_Amount_Mismatch()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingMomoTransactionAsync(db, amount: 100_000m);

        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", new
        {
            orderId = transaction.GatewayTransactionId,
            amount = "1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(1003);
    }

    [Fact]
    public async Task MomoIpn_Should_Return_0_When_Transaction_Already_Processed()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingMomoTransactionAsync(db, amount: 100_000m);
        transaction.Status = PaymentTransactionStatus.Success;
        db.PaymentTransactions.Update(transaction);
        await db.SaveChangesAsync();

        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", new
        {
            orderId = transaction.GatewayTransactionId,
            amount = "100000"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(0);
    }

    [Fact]
    public async Task MomoIpn_Should_Return_0_When_Booking_Was_Cancelled()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingMomoTransactionAsync(db, amount: 100_000m);
        transaction.Status = PaymentTransactionStatus.Cancelled;
        db.PaymentTransactions.Update(transaction);
        await db.SaveChangesAsync();

        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", new
        {
            orderId = transaction.GatewayTransactionId,
            amount = "100000"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(0);
    }

    [Fact]
    public async Task MomoIpn_Should_Return_1005_When_Signature_Invalid()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingMomoTransactionAsync(db, amount: 100_000m);

        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", new
        {
            partnerCode = "MOMO_TEST_PARTNER",
            requestId = "MOMO-REQ-1",
            orderId = transaction.GatewayTransactionId,
            orderInfo = $"Booking {transaction.BookingId}",
            orderType = "momo_wallet",
            amount = "100000",
            resultCode = "0",
            message = "Success",
            payType = "qr",
            transId = "123456",
            responseTime = "1710000000000",
            extraData = "",
            signature = "invalid-signature"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(1005);
    }

    [Fact]
    public async Task MomoIpn_Should_Return_0_And_Confirm_Booking_When_Valid()
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await SeedPendingMomoTransactionAsync(db, amount: 100_000m);

        var callbackFields = new Dictionary<string, string>
        {
            ["partnerCode"] = "MOMO_TEST_PARTNER",
            ["requestId"] = "MOMO-REQ-1",
            ["orderId"] = transaction.GatewayTransactionId,
            ["orderInfo"] = $"Booking {transaction.BookingId}",
            ["orderType"] = "momo_wallet",
            ["amount"] = "100000",
            ["resultCode"] = "0",
            ["message"] = "Success",
            ["payType"] = "qr",
            ["transId"] = "123456",
            ["responseTime"] = "1710000000000",
            ["extraData"] = ""
        };
        var signature = ComputeMomoSignature(callbackFields, "MOMO_TEST_SECRET_KEY");
        callbackFields["signature"] = signature;

        var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/payments/momo-ipn", callbackFields);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<MomoIpnResponse>();
        payload.Should().NotBeNull();
        payload!.ResultCode.Should().Be(0);

        var db2 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var txAfter = await db2.PaymentTransactions.AsNoTracking().SingleAsync(x => x.Id == transaction.Id);
        txAfter.Status.Should().Be(PaymentTransactionStatus.Success);
        var bookingAfter = await db2.Bookings.AsNoTracking().SingleAsync(x => x.Id == transaction.BookingId);
        bookingAfter.Status.Should().Be(BookingStatus.Confirmed);
    }

    private static string ComputeVnpaySignature(
        Dictionary<string, string> parameters,
        string secret)
    {
        var canonical = string.Join("&", parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(canonical);
        return Convert.ToHexStringLower(HMACSHA512.HashData(keyBytes, dataBytes));
    }

    private static string ComputeMomoSignature(
        Dictionary<string, string> fields,
        string secret)
    {
        var orderedKeys = new[]
        {
            "accessKey",
            "amount",
            "extraData",
            "message",
            "orderId",
            "orderInfo",
            "orderType",
            "partnerCode",
            "payType",
            "requestId",
            "responseTime",
            "resultCode",
            "transId"
        };

        var raw = string.Join("&", orderedKeys
            .Select(k =>
            {
                if (k == "accessKey")
                    return $"{k}=MOMO_TEST_ACCESS_KEY";
                return $"{k}={(fields.TryGetValue(k, out var value) ? value : string.Empty)}";
            }));

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToHexStringLower(HMACSHA256.HashData(keyBytes, dataBytes));
    }

    private static async Task<PaymentTransaction> SeedPendingVnpayTransactionAsync(AppDbContext db, decimal amount)
    {
        var cinema = IntegrationEntityBuilder.Cinema($"API VNPay Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"API VNPay Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"API-VNP-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var customer = IntegrationEntityBuilder.Customer($"session-{Guid.CreateVersion7():N}");
        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id, "VNPay IPN Tester");
        booking.OriginAmount = amount;
        booking.FinalAmount = amount;
        booking.Status = BookingStatus.Pending;

        var transaction = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = PaymentMethod.VnPay,
            Amount = amount,
            GatewayTransactionId = $"VNP-{Guid.CreateVersion7():N}",
            PaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Customers.Add(customer);
        db.Bookings.Add(booking);
        db.PaymentTransactions.Add(transaction);
        await db.SaveChangesAsync();

        return transaction;
    }

    private static async Task<PaymentTransaction> SeedPendingMomoTransactionAsync(AppDbContext db, decimal amount)
    {
        var cinema = IntegrationEntityBuilder.Cinema($"API Momo Cinema {Guid.CreateVersion7():N}");
        var movie = IntegrationEntityBuilder.Movie($"API Momo Movie {Guid.CreateVersion7():N}", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, $"API-MOMO-{Guid.CreateVersion7():N}", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);
        var customer = IntegrationEntityBuilder.Customer($"session-{Guid.CreateVersion7():N}");
        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customer.Id, "MoMo IPN Tester");
        booking.OriginAmount = amount;
        booking.FinalAmount = amount;
        booking.Status = BookingStatus.Pending;

        var transaction = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = PaymentMethod.Momo,
            Amount = amount,
            GatewayTransactionId = $"BOOKING-{booking.Id:N}".ToUpperInvariant(),
            PaymentUrl = "https://test-payment.momo.vn",
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Customers.Add(customer);
        db.Bookings.Add(booking);
        db.PaymentTransactions.Add(transaction);
        await db.SaveChangesAsync();

        return transaction;
    }
}
