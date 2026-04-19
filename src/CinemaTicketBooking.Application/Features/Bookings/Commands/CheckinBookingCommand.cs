namespace CinemaTicketBooking.Application.Features;

public class CheckInBookingCommand : ICommand
{
    public Guid BookingId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class CheckInBookingHandler(IUnitOfWork uow)
{
    public async Task Handle(CheckInBookingCommand command)
    {
        var booking = await uow.Bookings.GetByIdAsync(command.BookingId);

        if (booking is null)
            throw new InvalidOperationException(
                $"Booking with ID {command.BookingId} not found.");

        booking.CheckIn();
        uow.Bookings.Update(booking);
        await uow.CommitAsync();
    }
}