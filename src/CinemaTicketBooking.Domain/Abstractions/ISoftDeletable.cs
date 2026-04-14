namespace CinemaTicketBooking.Domain;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
    bool IsDeleted { get; }
}
