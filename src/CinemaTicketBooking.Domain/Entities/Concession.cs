namespace CinemaTicketBooking.Domain;

/// <summary>
/// Concession represents a snack or drink item available for purchase at the cinema
/// (e.g., popcorn, soda). Can be added to a Booking alongside tickets.
/// </summary>
public class Concession : AggregateRoot
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    // =============================================================
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new concession and raises a creation event.
    /// </summary>
    public static Concession Create(
        string name,
        decimal price,
        string imageUrl,
        bool isAvailable = true)
    {
        if (price < 0)
        {
            throw new ArgumentException("Concession price cannot be negative.", nameof(price));
        }

        var concession = new Concession
        {
            Name = name,
            Price = price,
            ImageUrl = imageUrl,
            IsAvailable = isAvailable
        };

        concession.RaiseEvent(new ConcessionCreated(
            ConcessionId: concession.Id,
            Name: concession.Name,
            Price: concession.Price,
            IsAvailable: concession.IsAvailable));

        return concession;
    }

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
        RaiseEvent(new ConcessionMarkedAvailable(Id));
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
        RaiseEvent(new ConcessionMarkedUnavailable(Id));
    }
}
