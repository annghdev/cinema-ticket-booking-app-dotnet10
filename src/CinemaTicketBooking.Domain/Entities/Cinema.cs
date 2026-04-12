namespace CinemaTicketBooking.Domain;

/// <summary>
/// Cinema represents a physical movie theater location.
/// Each Cinema has multiple Screens and is identified by name, address, and geo-coordinates.
/// </summary>
public class Cinema : AuditableEntity
{
    public required string Name { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Geo-coordinates for map display (e.g., "10.762622,106.660172").
    /// </summary>
    public string? Geo { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // =============================================================
    // State Transitions
    // =============================================================

    /// <summary>
    /// Toggles the cinema's active status and raises a domain event.
    /// Deactivating a cinema should trigger side effects on related screens and showtimes.
    /// </summary>
    public void ToggleActive()
    {
        IsActive = !IsActive;

        RaiseEvent(new CinemaStatusChanged(
            CinemaId: Id,
            Name: Name,
            Address: Address,
            IsActive: IsActive));
    }
}
