namespace CinemaTicketBooking.Domain;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
