namespace CinemaTicketBooking.Domain;

public class Cinema : AuditableEntity
{
    public required string Name { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Geo { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
