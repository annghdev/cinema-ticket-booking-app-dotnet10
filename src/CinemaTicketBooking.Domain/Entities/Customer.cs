namespace CinemaTicketBooking.Domain;

public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
}
