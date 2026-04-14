using CinemaTicketBooking.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketBooking.Infrastructure.Auth;

public class Role : IdentityRole<Guid>, IAggregateRoot
{
    public string DisplayName { get; set; } = string.Empty;
    public uint Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;

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
