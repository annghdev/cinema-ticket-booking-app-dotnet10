namespace CinemaTicketBooking.Application.Features;

public class CancelBookingCommand : ICommand
{
    public Guid BookingId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class CancelBookingHandler(IUnitOfWork uow)
{
    public async Task Handle(CancelBookingCommand command)
    {
        var booking = await uow.Bookings.GetByIdAsync(command.BookingId);

        if (booking is null)
            throw new InvalidOperationException(
                $"Booking with ID {command.BookingId} not found.");

        booking.Cancel();
        uow.Bookings.Update(booking);
        await uow.CommitAsync();
    }
}