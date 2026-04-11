namespace CinemaTicketBooking.Domain;

public abstract class BaseEntity: IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public uint Version { get; set; }
}
