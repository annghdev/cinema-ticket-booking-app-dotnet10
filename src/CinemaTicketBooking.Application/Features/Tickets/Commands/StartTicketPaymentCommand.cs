using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Moves ticket from locking to pending payment stage.
/// </summary>
public class StartTicketPaymentCommand : ICommand
{
    public Guid TicketId { get; set; }
    public Guid BookingId { get; set; }
    public string StartBy { get; set; } = string.Empty;
    public DateTimeOffset? PaymentExpiresAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles transition from locking to pending payment.
/// </summary>
public class StartTicketPaymentHandler(
    IUnitOfWork uow,
    ITicketLocker locker,
    IOptions<TicketLockingOptions> options)
{
    /// <summary>
    /// Starts payment phase and assigns payment expiration.
    /// </summary>
    public async Task Handle(StartTicketPaymentCommand cmd, CancellationToken ct)
    {
        var ticket = await uow.Tickets.GetByIdAsync(cmd.TicketId, ct);
        if (ticket is null)
        {
            throw new InvalidOperationException($"Ticket with ID '{cmd.TicketId}' not found.");
        }

        var paymentExpiresAt = cmd.PaymentExpiresAtUtc ?? DateTimeOffset.UtcNow.Add(options.Value.PaymentHoldDuration);
        ticket.StartPayment(cmd.BookingId, cmd.StartBy, paymentExpiresAt);
        uow.Tickets.Update(ticket);
        await uow.CommitAsync(ct);

        await locker.ReleaseAsync(cmd.TicketId, cmd.StartBy, ct);
    }
}

/// <summary>
/// Validates start-ticket-payment command payload.
/// </summary>
public class StartTicketPaymentValidator : AbstractValidator<StartTicketPaymentCommand>
{
    public StartTicketPaymentValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required.");

        RuleFor(x => x.StartBy)
            .NotEmpty().WithMessage("Payment actor is required.")
            .MaximumLength(MaxLengthConsts.SessionId);

        RuleFor(x => x.PaymentExpiresAtUtc)
            .Must(x => !x.HasValue || x.Value > DateTimeOffset.UtcNow)
            .WithMessage("Payment expiration must be in the future.");
    }
}
