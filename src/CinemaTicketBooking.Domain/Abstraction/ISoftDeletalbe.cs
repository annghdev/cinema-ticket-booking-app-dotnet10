namespace CinemaTicketBooking.Domain;

public interface ISoftDeletalbe
{
    DateTimeOffset? DeletedAt { get; set; }
    bool IsDeleted { get; }
}
