namespace CinemaTicketBooking.Domain;

public interface IAuditableEntity : IDefaultEntity, IAuditable, ISoftDeletable
{
}
