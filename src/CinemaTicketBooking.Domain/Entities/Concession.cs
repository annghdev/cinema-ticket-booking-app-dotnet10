namespace CinemaTicketBooking.Domain;

/// <summary>
/// Concession represents a snack or drink item available for purchase at the cinema
/// (e.g., popcorn, soda). Can be added to a Booking alongside tickets.
/// </summary>
public class Concession : AuditableEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    // =============================================================
    // State Transitions
    // =============================================================

    /// <summary>
    /// Toggles the concession's availability and raises a domain event.
    /// Side effects: update real-time menu display for customers.
    /// </summary>
    public void ToggleAvailability()
    {
        IsAvailable = !IsAvailable;

        RaiseEvent(new ConcessionAvailabilityChanged(
            ConcessionId: Id,
            Name: Name,
            IsAvailable: IsAvailable,
            Price: Price));
    }
}
