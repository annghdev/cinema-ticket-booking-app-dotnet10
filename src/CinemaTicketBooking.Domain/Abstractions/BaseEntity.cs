using CinemaTicketBooking.Domain.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketBooking.Domain;

public abstract class BaseEntity: IDefaultEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public uint Version { get; set; }

    private readonly List<IDomainEvent> _events = [];
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> Events => _events.AsReadOnly();

    public void RaiseEvent(IDomainEvent @event)
    {
        _events.Add(@event);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }
}
