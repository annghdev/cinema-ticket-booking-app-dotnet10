namespace CinemaTicketBooking.Domain;

public record BaseDomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
