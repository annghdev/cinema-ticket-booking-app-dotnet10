using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Features.Bookings.Commands;

/// <summary>
/// Starts a new payment attempt for a pending booking after a previous attempt failed or expired.
/// The client supplies the same session id used at checkout so only the booking owner can retry.
/// </summary>
public class RetryPaymentCommand : ICommand
{
    public Guid BookingId { get; set; }

    /// <summary>
    /// Session id of the customer who created the booking (must match <see cref="Booking.Customer"/>).
    /// </summary>
    public string CustomerSessionId { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Creates a new pending <see cref="PaymentTransaction"/> and returns fresh gateway presentation data.
/// </summary>
public class RetryPaymentHandler(
    IUnitOfWork uow,
    IOptions<TicketLockingOptions> options,
    IPaymentServiceFactory paymentServiceFactory)
{
    /// <summary>
    /// Validates booking state, extends ticket payment holds, calls the gateway, and persists the new transaction.
    /// </summary>
    public async Task<CreateBookingResponse> Handle(
        RetryPaymentCommand command,
        CancellationToken ct)
    {
        // 1. Load booking aggregate and authorize the caller by session.
        var booking = await uow.Bookings.LoadFullAsync(command.BookingId, ct)
            ?? throw new InvalidOperationException($"Booking '{command.BookingId}' was not found.");

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending bookings can retry payment.");
        }

        if (booking.Customer is null
            || !string.Equals(booking.Customer.SessionId, command.CustomerSessionId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Customer session is not authorized to retry payment for this booking.");
        }

        // 2. Ensure no other payment attempt is still awaiting gateway confirmation.
        var existingPending = await uow.PaymentTransactions.GetPendingByBookingIdAsync(command.BookingId, ct);
        if (existingPending is not null)
        {
            throw new InvalidOperationException(
                "A payment is already in progress for this booking. Complete or cancel it before retrying.");
        }

        // 3. Extend pending-payment holds so recovery jobs do not release seats mid-retry.
        var paymentExpiresAt = DateTimeOffset.UtcNow.Add(options.Value.PaymentHoldDuration);
        foreach (var bt in booking.Tickets)
        {
            if (bt.Ticket is null)
                throw new InvalidOperationException("Booking ticket must have an associated ticket.");

            if (bt.Ticket.Status != TicketStatus.PendingPayment)
            {
                throw new InvalidOperationException(
                    "All tickets on this booking must be in pending payment to retry.");
            }

            bt.Ticket.ExtendPaymentHold(paymentExpiresAt);
            uow.Tickets.Update(bt.Ticket);
        }

        // 4. Start payment on the selected gateway.
        var method = Enum.Parse<PaymentMethod>(command.PaymentMethod, ignoreCase: true);
        var paymentService = paymentServiceFactory.GetService(method);

        var paymentResult = await paymentService.CreatePaymentAsync(
            new CreatePaymentRequest(
                BookingId: booking.Id,
                Amount: booking.FinalAmount,
                OrderDescription: $"Booking {booking.Id} (retry)",
                CustomerEmail: booking.Email,
                ReturnUrl: command.ReturnUrl,
                IpAddress: command.IpAddress),
            ct);

        if (!paymentResult.Success)
        {
            throw new InvalidOperationException(
                $"Payment gateway failed: {paymentResult.ErrorMessage}");
        }

        // 5. Persist the new payment transaction.
        var transaction = new PaymentTransaction
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            Method = method,
            GatewayTransactionId = paymentResult.GatewayTransactionId,
            RedirectBehavior = paymentResult.RedirectBehavior,
            PaymentUrl = paymentResult.PaymentUrl,
            Amount = booking.FinalAmount,
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = paymentExpiresAt
        };
        uow.PaymentTransactions.Add(transaction);
        await uow.CommitAsync(ct);

        return new CreateBookingResponse(
            BookingId: booking.Id,
            PaymentExpiresAt: paymentExpiresAt,
            OriginAmount: booking.OriginAmount,
            FinalAmount: booking.FinalAmount,
            PaymentStatus: "pending_payment",
            PaymentUrl: paymentResult.PaymentUrl,
            RedirectBehavior: paymentResult.RedirectBehavior,
            PaymentTransactionId: transaction.Id);
    }
}

/// <summary>
/// Validates retry payment input.
/// </summary>
public class RetryPaymentCommandValidator : AbstractValidator<RetryPaymentCommand>
{
    public RetryPaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required.");

        RuleFor(x => x.CustomerSessionId)
            .NotEmpty()
            .WithMessage("Customer session ID is required.")
            .MaximumLength(MaxLengthConsts.SessionId);

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .WithMessage("Return URL is required.")
            .MaximumLength(MaxLengthConsts.Url);

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .WithMessage("IP address is required.");
    }
}
