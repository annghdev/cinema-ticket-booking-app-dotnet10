namespace CinemaTicketBooking.Domain;

public abstract class AggregateRoot : DefaultEntity, IAuditable, ISoftDeletable
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
