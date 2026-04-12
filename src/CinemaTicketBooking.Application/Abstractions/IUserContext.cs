namespace CinemaTicketBooking.Application;

public interface IUserContext
{
    Guid UserId { get; }
    string UserName { get; }
    bool IsInRole(string role);
}
