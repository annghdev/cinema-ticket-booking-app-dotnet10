namespace CinemaTicketBooking.Infrastructure.Auth;

public class AccountOptions
{
    public const string SectionName = "Account";
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public int RequiredLength { get; set; } = 6;
}
