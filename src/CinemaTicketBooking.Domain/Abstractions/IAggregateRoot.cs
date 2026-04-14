namespace CinemaTicketBooking.Domain.Abstractions;

public interface IAggregateRoot : IDefaultEntity, IAuditable, ISoftDeletable
{
}
