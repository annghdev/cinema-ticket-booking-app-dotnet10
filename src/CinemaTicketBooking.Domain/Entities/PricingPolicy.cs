namespace CinemaTicketBooking.Domain;

/// <summary>
/// Pricing policy defines the ticket price for a specific combination of
/// Screen type and Seat type. Stored in DB — admin can update without redeployment.
///
/// Formula: TicketPrice = BasePrice × ScreenCoefficient
///
/// Example pricing policies:
/// | ScreenType | SeatType  | BasePrice | ScreenCoefficient | Final Price |
/// |------------|-----------|-----------|-------------------|-------------|
/// | TwoD       | Regular   | 50,000    | 1.0               | 50,000      |
/// | TwoD       | VIP       | 100,000   | 1.0               | 100,000     |
/// | ThreeD     | Regular   | 50,000    | 1.5               | 75,000      |
/// | ThreeD     | VIP       | 100,000   | 1.5               | 150,000     |
/// | IMAX       | Regular   | 50,000    | 2.0               | 100,000     |
/// | IMAX       | SweetBox  | 150,000   | 2.0               | 300,000     |
/// </summary>
public class PricingPolicy : AuditableEntity
{
    /// <summary>
    /// Optional: scope to a specific Cinema. Null = applies to all cinemas (default policy).
    /// </summary>
    public Guid? CinemaId { get; set; }
    public Cinema? Cinema { get; set; }

    public ScreenType ScreenType { get; set; }
    public SeatType SeatType { get; set; }

    /// <summary>
    /// Base price for this seat type (before screen coefficient).
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Multiplier based on screen type (e.g., IMAX = 2.0, 3D = 1.5).
    /// </summary>
    public decimal ScreenCoefficient { get; set; } = 1.0m;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Calculates the final ticket price.
    /// </summary>
    public decimal CalculatePrice() => BasePrice * ScreenCoefficient;
}
