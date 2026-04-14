namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Schedules auto-release when a ticket enters pending payment stage.
/// </summary>
public class TicketPendingPaymentHandler(IMessageBus bus)
{
    /// <summary>
    /// Persists delayed payment-timeout release through Wolverine scheduling.
    /// </summary>
    public async Task Handle(TicketPendingPayment domainEvent, CancellationToken ct)
    {
        await bus.ScheduleAsync(
            new ReleaseTicketPaymentByTimeoutCommand
            {
                TicketId = domainEvent.TicketId,
                BookingId = domainEvent.BookingId,
                CorrelationId = domainEvent.BookingId.ToString()
            },
            domainEvent.PaymentExpiresAt);
    }
}
