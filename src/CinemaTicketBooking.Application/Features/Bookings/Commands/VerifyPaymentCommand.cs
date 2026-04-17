namespace CinemaTicketBooking.Application.Features.Bookings.Commands;

/// <summary>
/// Verifies a payment gateway callback/return-URL result.
/// The caller (API endpoint) converts HTTP query-string / form data into
/// the gateway-agnostic GatewayResponseParams dictionary before dispatching.
/// </summary>
public class VerifyPaymentCommand : ICommand
{
    public Guid BookingId { get; set; }
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Raw key-value pairs forwarded from the gateway callback.
    /// The IPaymentService implementation uses these to verify the signature.
    /// </summary>
    public Dictionary<string, string> GatewayResponseParams { get; set; } = [];
}

/// <summary>
/// Handles payment verification, booking confirmation on success,
/// and retry signalling on failure.
/// </summary>
public class VerifyPaymentHandler(
    IUnitOfWork uow,
    IPaymentServiceFactory paymentServiceFactory,
    IQrCodeGenerator qrCodeGenerator)
{
    /// <summary>
    /// Verifies payment, updates transaction status, and confirms or signals retry for the booking.
    /// </summary>
    public async Task<VerifyPaymentResponse> Handle(
        VerifyPaymentCommand command,
        CancellationToken ct)
    {
        // 1. Load the pending payment transaction for this booking.
        var transaction = await uow.PaymentTransactions.GetPendingByBookingIdAsync(command.BookingId, ct)
            ?? throw new InvalidOperationException(
                $"No pending payment transaction found for booking '{command.BookingId}'.");

        // 2. Guard: ensure the gateway transaction ID matches.
        if (transaction.GatewayTransactionId != command.GatewayTransactionId)
        {
            throw new InvalidOperationException(
                "Gateway transaction ID mismatch. Possible replay or tampered callback.");
        }

        // 3. Resolve the payment service and verify the signature + result.
        var method = Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true);
        var paymentService = paymentServiceFactory.GetService(method);

        var confirmResult = await paymentService.ConfirmPaymentAsync(
            new ConfirmPaymentRequest(
                BookingId: command.BookingId,
                GatewayTransactionId: command.GatewayTransactionId,
                GatewayResponseParams: command.GatewayResponseParams), ct);

        // 4. Branch: success → confirm booking; failure → mark failed and signal retry.
        if (confirmResult.IsSuccess)
        {
            return await HandleSuccessAsync(transaction, ct);
        }

        return await HandleFailureAsync(transaction, confirmResult, ct);
    }

    // =============================================
    // Payment outcome handlers
    // =============================================

    /// <summary>
    /// Marks transaction as successful, confirms booking and its tickets, and commits atomically.
    /// </summary>
    private async Task<VerifyPaymentResponse> HandleSuccessAsync(
        PaymentTransaction transaction,
        CancellationToken ct)
    {
        // 1. Update transaction to Success.
        transaction.Status = PaymentTransactionStatus.Success;
        transaction.PaidAt = DateTimeOffset.UtcNow;
        uow.PaymentTransactions.Update(transaction);

        // 2. Load full booking aggregate and confirm (marks tickets as Sold).
        var booking = await uow.Bookings.LoadFullAsync(transaction.BookingId, ct)
            ?? throw new InvalidOperationException(
                $"Booking '{transaction.BookingId}' not found.");

        booking.Confirm();
        uow.Bookings.Update(booking);

        // 3. Persist tickets state changes.
        foreach (var bt in booking.Tickets)
        {
            if (bt.Ticket is not null)
                uow.Tickets.Update(bt.Ticket);
        }

        // 4. Generate check-in code.
        booking.QrCode = qrCodeGenerator.GenerateCode(booking.Id.ToString());

        // 5. Atomic commit: Transaction(Success) + Booking(Confirmed) + Tickets(Sold).
        await uow.CommitAsync(ct);

        return new VerifyPaymentResponse(
            BookingId: booking.Id,
            PaymentTransactionId: transaction.Id,
            IsSuccess: true,
            CheckinQrCode: booking.QrCode,
            Status: "confirmed");
    }

    /// <summary>
    /// Marks transaction as failed and returns available gateways so the client
    /// can let the user retry with a different payment method.
    /// </summary>
    private async Task<VerifyPaymentResponse> HandleFailureAsync(
        PaymentTransaction transaction,
        ConfirmPaymentResult confirmResult,
        CancellationToken ct)
    {
        // 1. Update transaction to Failed.
        transaction.Status = PaymentTransactionStatus.Failed;
        transaction.GatewayResponseRaw = confirmResult.ErrorMessage;
        uow.PaymentTransactions.Update(transaction);

        // 2. Commit the failed transaction state only (booking stays Pending).
        await uow.CommitAsync(ct);

        // 3. Provide available gateways so client can offer retry with a different method.
        var availableGateways = paymentServiceFactory.GetAvailableOptions();

        return new VerifyPaymentResponse(
            BookingId: transaction.BookingId,
            PaymentTransactionId: transaction.Id,
            IsSuccess: false,
            CheckinQrCode: null,
            Status: "payment_failed",
            ErrorMessage: confirmResult.ErrorMessage ?? "Payment verification failed.",
            CanRetry: true,
            AvailableGateways: availableGateways);
    }
}

/// <summary>
/// Validates the VerifyPaymentCommand input.
/// </summary>
public class VerifyPaymentCommandValidator : AbstractValidator<VerifyPaymentCommand>
{
    public VerifyPaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required.");

        RuleFor(x => x.GatewayTransactionId)
            .NotEmpty()
            .WithMessage("Gateway transaction ID is required.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.");

        RuleFor(x => x.GatewayResponseParams)
            .NotNull()
            .WithMessage("Gateway response parameters are required.");
    }
}