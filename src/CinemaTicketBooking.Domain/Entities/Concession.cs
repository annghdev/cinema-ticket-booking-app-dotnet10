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
    // State transitions
    // =============================================================

    /// <summary>
    /// Marks the concession as available for purchase. No-op when already available (idempotent).
    /// </summary>
    public void MarkAsAvailable()
    {
        if (IsAvailable)
        {
            return;
        }

        IsAvailable = true;
        RaiseEvent(new ConcessionMarkedAvailable(Id, Name, Price));
    }

    /// <summary>
    /// Marks the concession as unavailable (e.g., out of stock). No-op when already unavailable (idempotent).
    /// </summary>
    public void MarkAsUnavailable()
    {
        if (!IsAvailable)
        {
            return;
        }

        IsAvailable = false;
        RaiseEvent(new ConcessionMarkedUnavailable(Id, Name, Price));
    }
}
