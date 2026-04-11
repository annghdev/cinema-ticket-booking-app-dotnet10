namespace CinemaTicketBooking.Domain;

public class ShowTime : AuditableEntity
{
    public Guid MovieId { get; set; }
    public Movie? Movie { get; set; }
    public Guid ScreenId { get; set; }
    public Screen? Screen { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public ShowTimeStatus Status { get; set; }
}

//public enum ShowTimeStatus
//{
//    Ongoing,
//    Showing,
//    Completed,
//    Cancelled
//}
