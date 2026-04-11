namespace CinemaTicketBooking.Domain;

public class Concession : AuditableEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
