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
public class PricingPolicy : AggregateRoot
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

    /// <summary>
    /// Surcharge multiplier applied if the showtime falls on a weekend (Saturday/Sunday).
    /// </summary>
    public decimal WeekendCoefficient { get; set; } = 1.0m;

    public bool IsActive { get; set; } = true;

    // =============================================================
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new pricing policy and raises a creation event.
    /// </summary>
    public static PricingPolicy Create(
        Guid? cinemaId,
        ScreenType screenType,
        SeatType seatType,
        decimal basePrice,
        decimal screenCoefficient,
        decimal weekendCoefficient = 1.0m,
        bool isActive = true)
    {
        if (basePrice < 0)
        {
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));
        }

        if (screenCoefficient <= 0)
        {
            throw new ArgumentException("Screen coefficient must be positive.", nameof(screenCoefficient));
        }

        if (weekendCoefficient <= 0)
        {
            throw new ArgumentException("Weekend coefficient must be positive.", nameof(weekendCoefficient));
        }

        var pricingPolicy = new PricingPolicy
        {
            CinemaId = cinemaId,
            ScreenType = screenType,
            SeatType = seatType,
            BasePrice = basePrice,
            ScreenCoefficient = screenCoefficient,
            WeekendCoefficient = weekendCoefficient,
            IsActive = isActive
        };

        pricingPolicy.RaiseEvent(new PricingPolicyCreated(
            PricingPolicyId: pricingPolicy.Id,
            CinemaId: pricingPolicy.CinemaId,
            ScreenType: pricingPolicy.ScreenType,
            SeatType: pricingPolicy.SeatType,
            BasePrice: pricingPolicy.BasePrice,
            ScreenCoefficient: pricingPolicy.ScreenCoefficient,
            IsActive: pricingPolicy.IsActive));

        return pricingPolicy;
    }

    /// <summary>
    /// Updates basic pricing policy fields and raises an update event.
    /// </summary>
    public void UpdateBasicInfo(
        Guid? cinemaId,
        ScreenType screenType,
        SeatType seatType,
        decimal basePrice,
        decimal screenCoefficient,
        decimal weekendCoefficient,
        bool isActive)
    {
        if (basePrice < 0)
        {
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));
        }

        if (screenCoefficient <= 0)
        {
            throw new ArgumentException("Screen coefficient must be positive.", nameof(screenCoefficient));
        }

        if (weekendCoefficient <= 0)
        {
            throw new ArgumentException("Weekend coefficient must be positive.", nameof(weekendCoefficient));
        }

        CinemaId = cinemaId;
        ScreenType = screenType;
        SeatType = seatType;
        BasePrice = basePrice;
        ScreenCoefficient = screenCoefficient;
        WeekendCoefficient = weekendCoefficient;
        IsActive = isActive;

        RaiseEvent(new PricingPolicyBasicInfoUpdated(
            PricingPolicyId: Id,
            CinemaId: CinemaId,
            ScreenType: ScreenType,
            SeatType: SeatType,
            BasePrice: BasePrice,
            ScreenCoefficient: ScreenCoefficient,
            WeekendCoefficient: WeekendCoefficient,
            IsActive: IsActive));
    }

    /// <summary>
    /// Calculates the final ticket price.
    /// Formula: TicketPrice = BasePrice × ScreenCoefficient × (isWeekend ? WeekendCoefficient : 1.0)
    /// </summary>
    public decimal CalculatePrice(bool isWeekend = false) 
    {
        return BasePrice * ScreenCoefficient * (isWeekend ? WeekendCoefficient : 1.0m);
    }

    // =============================================================
    // Update Pricing
    // =============================================================

    /// <summary>
    /// Updates the pricing values and raises a domain event with old and new values.
    /// Side effects: invalidate pricing cache, recalculate affected future showtime tickets.
    /// </summary>
    public void UpdatePricing(decimal newBasePrice, decimal newScreenCoefficient)
    {
        if (newBasePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(newBasePrice));

        if (newScreenCoefficient <= 0)
            throw new ArgumentException("Screen coefficient must be positive.", nameof(newScreenCoefficient));

        var oldBasePrice = BasePrice;
        var oldScreenCoefficient = ScreenCoefficient;

        BasePrice = newBasePrice;
        ScreenCoefficient = newScreenCoefficient;

        RaiseEvent(new PricingPolicyUpdated(
            PricingPolicyId: Id,
            CinemaId: CinemaId,
            ScreenType: ScreenType,
            SeatType: SeatType,
            OldBasePrice: oldBasePrice,
            NewBasePrice: newBasePrice,
            OldScreenCoefficient: oldScreenCoefficient,
            NewScreenCoefficient: newScreenCoefficient));
    }

    /// <summary>
    /// Activates this pricing policy. No-op when already active (idempotent).
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        RaiseEvent(new PricingPolicyActivated(Id, CinemaId, ScreenType, SeatType));
    }

    /// <summary>
    /// Deactivates this pricing policy. No-op when already inactive (idempotent).
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RaiseEvent(new PricingPolicyDeactivated(Id, CinemaId, ScreenType, SeatType));
    }

    /// <summary>
    /// Marks this pricing policy as deleted (soft delete handled by infrastructure).
    /// </summary>
    public void MarkAsDeleted()
    {
        RaiseEvent(new PricingPolicyDeleted(Id, CinemaId, ScreenType, SeatType));
    }
}
