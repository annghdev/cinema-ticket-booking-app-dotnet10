namespace CinemaTicketBooking.Application.Features;

public class CancelBookingCommand : ICommand
{
    public Guid BookingId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class CancelBookingHandler(IUnitOfWork uow)
{
    public async Task Handle(CancelBookingCommand command, CancellationToken ct)
    {
        var booking = await uow.Bookings.LoadFullAsync(command.BookingId, ct);

        if (booking is null)
            throw new InvalidOperationException(
                $"Booking with ID {command.BookingId} not found.");

        // 1. Cancel booking aggregate and release tickets.
        booking.Cancel();
        // uow.Bookings.Update(booking);

        // foreach (var bookingTicket in booking.Tickets)
        // {
        //     if (bookingTicket.Ticket is not null)
        //     {
        //         uow.Tickets.Update(bookingTicket.Ticket);
        //     }
        // }

        // 2. Close any in-flight pending payment attempts.
        var pendingTransactions = await uow.PaymentTransactions.GetAllPendingByBookingIdAsync(command.BookingId, ct);
        foreach (var transaction in pendingTransactions)
        {
            transaction.Status = PaymentTransactionStatus.Cancelled;
            transaction.GatewayResponseRaw = "Payment cancelled because booking was cancelled by customer.";
            uow.PaymentTransactions.Update(transaction);
        }

        // 3. Persist booking, ticket and payment transaction changes atomically.
        await uow.CommitAsync(ct);
    }
}