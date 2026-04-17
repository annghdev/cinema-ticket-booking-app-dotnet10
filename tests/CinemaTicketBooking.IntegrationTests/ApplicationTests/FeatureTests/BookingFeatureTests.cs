using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Features.Bookings.Commands;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

public sealed class BookingFeatureTests(PostgresContainerFixture databaseFixture)
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
        response.PaymentOptions.Should().NotBeNullOrEmpty();
        response.PaymentOptions!.Should().Contain(x => x.Method == "None");
    }

    [Fact]
    public async Task CreateBooking_Should_UseSharedPaymentExpiry_ForAllSelectedTickets_And_CreatePaymentTransaction()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();
        var concessionId = await SeedConcessionAsync();

        var response = await InvokeAsync<CreateBookingResponse>(new CreateBookingCommand
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
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
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
        response.PaymentUrl.Should().NotBeNullOrEmpty();
        response.RedirectBehavior.Should().Be(PaymentRedirectBehavior.QrCode);
        response.PaymentTransactionId.Should().NotBeNull();

        var paymentTransaction = await db.PaymentTransactions
            .SingleAsync(x => x.BookingId == response.BookingId);
        paymentTransaction.Method.Should().Be(PaymentMethod.None);
        paymentTransaction.Status.Should().Be(PaymentTransactionStatus.Pending);
        paymentTransaction.Amount.Should().Be(booking.FinalAmount);
    }

    [Fact]
    public async Task VerifyPayment_Should_ConfirmBooking_SetQrAndSoldTickets_When_GatewayConfirms()
    {
        var (bookingId, gatewayTxId, paymentTxId, _) = await ArrangePendingBookingForVerifyPaymentAsync();

        var verifyResponse = await InvokeAsync<VerifyPaymentResponse>(new VerifyPaymentCommand
        {
            BookingId = bookingId,
            GatewayTransactionId = gatewayTxId,
            PaymentMethod = "None",
            GatewayResponseParams = [],
            CorrelationId = "it-verify-payment-success"
        });

        verifyResponse.IsSuccess.Should().BeTrue();
        verifyResponse.Status.Should().Be("confirmed");
        verifyResponse.PaymentTransactionId.Should().Be(paymentTxId);
        verifyResponse.CheckinQrCode.Should().NotBeNullOrEmpty();
        verifyResponse.CheckinQrCode!.Should().StartWith("data:image/png;base64,");
        verifyResponse.CheckinQrCode.Length.Should().BeLessOrEqualTo(MaxLengthConsts.QrCode);
        verifyResponse.CanRetry.Should().BeFalse();

        await using var db = CreateDbContext();
        var bookingAfter = await db.Bookings
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Ticket)
            .SingleAsync(x => x.Id == bookingId);

        bookingAfter.Status.Should().Be(BookingStatus.Confirmed);
        bookingAfter.QrCode.Should().Be(verifyResponse.CheckinQrCode);
        bookingAfter.Tickets.Should().ContainSingle();
        bookingAfter.Tickets.Single().Ticket!.Status.Should().Be(TicketStatus.Sold);

        var paymentAfter = await db.PaymentTransactions.SingleAsync(x => x.Id == paymentTxId);
        paymentAfter.Status.Should().Be(PaymentTransactionStatus.Success);
        paymentAfter.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyPayment_Should_Throw_When_GatewayTransactionIdDoesNotMatchPending()
    {
        var (bookingId, gatewayTxId, _, _) = await ArrangePendingBookingForVerifyPaymentAsync();

        var act = async () => await InvokeAsync<VerifyPaymentResponse>(new VerifyPaymentCommand
        {
            BookingId = bookingId,
            GatewayTransactionId = "WRONG-" + gatewayTxId,
            PaymentMethod = "None",
            GatewayResponseParams = [],
            CorrelationId = "it-verify-payment-mismatch"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Gateway transaction ID mismatch*");
    }

    [Fact]
    public async Task VerifyPayment_Should_MarkTransactionFailed_And_KeepBookingPending_When_CallbackSignatureInvalid()
    {
        var (bookingId, gatewayTxId, paymentTxId, _) = await ArrangePendingBookingForVerifyPaymentAsync();

        var verifyResponse = await InvokeAsync<VerifyPaymentResponse>(new VerifyPaymentCommand
        {
            BookingId = bookingId,
            GatewayTransactionId = gatewayTxId,
            PaymentMethod = "None",
            GatewayResponseParams = new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "00",
                ["vnp_TxnRef"] = gatewayTxId,
                ["vnp_SecureHash"] = "deadbeef"
            },
            CorrelationId = "it-verify-payment-bad-signature"
        });

        verifyResponse.IsSuccess.Should().BeFalse();
        verifyResponse.Status.Should().Be("payment_failed");
        verifyResponse.CanRetry.Should().BeTrue();
        verifyResponse.CheckinQrCode.Should().BeNull();
        verifyResponse.AvailableGateways.Should().NotBeNullOrEmpty();
        verifyResponse.ErrorMessage.Should().Contain("signature");

        await using var db = CreateDbContext();
        var bookingAfter = await db.Bookings
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Ticket)
            .SingleAsync(x => x.Id == bookingId);

        bookingAfter.Status.Should().Be(BookingStatus.Pending);
        bookingAfter.QrCode.Should().BeNull();
        bookingAfter.Tickets.Single().Ticket!.Status.Should().Be(TicketStatus.PendingPayment);

        var paymentAfter = await db.PaymentTransactions.SingleAsync(x => x.Id == paymentTxId);
        paymentAfter.Status.Should().Be(PaymentTransactionStatus.Failed);
    }

    [Fact]
    public async Task RetryPayment_Should_CreateNewPendingTransaction_When_PreviousVerificationFailed()
    {
        var (bookingId, sessionId) = await ArrangeBookingWithFailedPaymentAsync();

        var retryResponse = await InvokeAsync<CreateBookingResponse>(new RetryPaymentCommand
        {
            BookingId = bookingId,
            CustomerSessionId = sessionId,
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
            CorrelationId = "it-retry-payment-after-fail"
        });

        retryResponse.BookingId.Should().Be(bookingId);
        retryResponse.PaymentStatus.Should().Be("pending_payment");
        retryResponse.PaymentUrl.Should().NotBeNullOrEmpty();
        retryResponse.PaymentTransactionId.Should().NotBeNull();
        retryResponse.RedirectBehavior.Should().Be(PaymentRedirectBehavior.QrCode);

        await using var db = CreateDbContext();
        var transactions = await db.PaymentTransactions
            .Where(x => x.BookingId == bookingId)
            .ToListAsync();

        transactions.Should().HaveCount(2);
        transactions.Should().ContainSingle(x => x.Status == PaymentTransactionStatus.Failed);
        var pendingRetry = transactions.Single(x => x.Status == PaymentTransactionStatus.Pending);
        pendingRetry.Id.Should().Be(retryResponse.PaymentTransactionId!.Value);

        var booking = await db.Bookings
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Ticket)
            .SingleAsync(x => x.Id == bookingId);

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.Tickets.Single().Ticket!.PaymentExpiresAt
            .Should()
            .BeCloseTo(retryResponse.PaymentExpiresAt, precision: TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task RetryPayment_Should_Throw_When_PaymentStillPending()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();

        var createResponse = await InvokeAsync<CreateBookingResponse>(new CreateBookingCommand
        {
            ShowTimeId = seed.ShowTimeId,
            CustomerSessionId = seed.SessionId,
            CustomerName = "Retry Blocked",
            CustomerPhoneNumber = "0123456789",
            CustomerEmail = "retry.blocked@example.com",
            SelectedTicketIds = [seed.TicketsBySeatCode["A1"]],
            Concessions = [],
            DiscountAmount = 0,
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
            CorrelationId = "it-retry-while-pending"
        });

        var act = async () => await InvokeAsync<CreateBookingResponse>(new RetryPaymentCommand
        {
            BookingId = createResponse.BookingId,
            CustomerSessionId = seed.SessionId,
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
            CorrelationId = "it-retry-while-pending-2"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already in progress*");
    }

    [Fact]
    public async Task RetryPayment_Should_Throw_When_CustomerSessionDoesNotMatch()
    {
        var (bookingId, _) = await ArrangeBookingWithFailedPaymentAsync();

        var act = async () => await InvokeAsync<CreateBookingResponse>(new RetryPaymentCommand
        {
            BookingId = bookingId,
            CustomerSessionId = "another-session",
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
            CorrelationId = "it-retry-wrong-session"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not authorized*");
    }

    private async Task<(Guid BookingId, string SessionId)> ArrangeBookingWithFailedPaymentAsync()
    {
        var (bookingId, gatewayTxId, _, sessionId) = await ArrangePendingBookingForVerifyPaymentAsync();

        await InvokeAsync<VerifyPaymentResponse>(new VerifyPaymentCommand
        {
            BookingId = bookingId,
            GatewayTransactionId = gatewayTxId,
            PaymentMethod = "None",
            GatewayResponseParams = new Dictionary<string, string>
            {
                ["vnp_ResponseCode"] = "00",
                ["vnp_SecureHash"] = "invalid"
            },
            CorrelationId = "it-arrange-failed-payment"
        });

        return (bookingId, sessionId);
    }

    private async Task<(Guid BookingId, string GatewayTransactionId, Guid PaymentTransactionId, string SessionId)>
        ArrangePendingBookingForVerifyPaymentAsync()
    {
        await ResetDatabaseAsync();
        var seed = await SeedCheckoutGraphAsync();

        var createResponse = await InvokeAsync<CreateBookingResponse>(new CreateBookingCommand
        {
            ShowTimeId = seed.ShowTimeId,
            CustomerSessionId = seed.SessionId,
            CustomerName = "Verify Payment IT",
            CustomerPhoneNumber = "0123456789",
            CustomerEmail = "verify.payment.it@example.com",
            SelectedTicketIds = [seed.TicketsBySeatCode["A1"]],
            Concessions = [],
            DiscountAmount = 0,
            PaymentMethod = "None",
            ReturnUrl = "https://localhost/checkout/return",
            IpAddress = "127.0.0.1",
            CorrelationId = "it-arrange-pending-booking"
        });

        await using var db = CreateDbContext();
        var transaction = await db.PaymentTransactions
            .AsNoTracking()
            .SingleAsync(x => x.BookingId == createResponse.BookingId);

        return (createResponse.BookingId, transaction.GatewayTransactionId, transaction.Id, seed.SessionId);
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
