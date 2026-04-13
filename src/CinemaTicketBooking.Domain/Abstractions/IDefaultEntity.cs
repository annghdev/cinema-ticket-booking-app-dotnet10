namespace CinemaTicketBooking.Domain.Abstractions;

public interface IDefaultEntity : IEntity
{
    DateTimeOffset CreatedAt { get; set; }
    uint Version { get; set; }
    IReadOnlyCollection<IDomainEvent> Events { get; }

    public void RaiseEvent(IDomainEvent @event);
    public void ClearEvents();
}
