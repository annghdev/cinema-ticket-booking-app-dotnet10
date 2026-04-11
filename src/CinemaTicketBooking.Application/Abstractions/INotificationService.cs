namespace CinemaTicketBooking.Application;

public interface INotificationService
{
    Task SendAsync(object message, CancellationToken ct = default);
}
