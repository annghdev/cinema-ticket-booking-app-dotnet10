namespace CinemaTicketBooking.Domain;

public interface ITrackable
{
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
    string? CreatedBy { get; set; }
}
