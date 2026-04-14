namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Scheduled command to release pending-payment ticket when payment timeout expires.
/// </summary>
public class ReleaseTicketPaymentByTimeoutCommand : ICommand
{
    public Guid TicketId { get; set; }
    public Guid BookingId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles delayed auto-release for timed-out pending payment stage.
/// </summary>
public class ReleaseTicketPaymentByTimeoutHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Performs idempotent release only when ticket is still pending payment for the same booking.
    /// </summary>
    public async Task Handle(ReleaseTicketPaymentByTimeoutCommand cmd, CancellationToken ct)
    {
        var ticket = await uow.Tickets.GetByIdAsync(cmd.TicketId, ct);
        var now = DateTimeOffset.UtcNow;
        if (ticket is null
            || ticket.Status != TicketStatus.PendingPayment
            || ticket.BookingId != cmd.BookingId
            || !ticket.PaymentExpiresAt.HasValue
            || ticket.PaymentExpiresAt > now)
        {
            return;
        }

        ticket.Release();
        uow.Tickets.Update(ticket);
        await uow.CommitAsync(ct);
    }
}
