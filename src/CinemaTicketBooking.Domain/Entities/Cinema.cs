namespace CinemaTicketBooking.Domain;

/// <summary>
/// Cinema represents a physical movie theater location.
/// Each Cinema has multiple Screens and is identified by name, address, and geo-coordinates.
/// </summary>
public class Cinema : AggregateRoot
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
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new cinema and raises a creation event.
    /// </summary>
    public static Cinema Create(
        string name,
        string thumbnailUrl,
        string? geo,
        string address,
        bool isActive)
    {
        var cinema = new Cinema
        {
            Name = name,
            ThumbnailUrl = thumbnailUrl,
            Geo = geo,
            Address = address,
            IsActive = isActive
        };

        cinema.RaiseEvent(new CinemaCreated(cinema.Id, cinema.Name, cinema.Address, cinema.IsActive));
        return cinema;
    }

    /// <summary>
    /// Updates basic cinema fields and raises an update event.
    /// </summary>
    public void UpdateBasicInfo(
        string name,
        string thumbnailUrl,
        string? geo,
        string address)
    {
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Geo = geo;
        Address = address;

        RaiseEvent(new CinemaBasicInfoUpdated(Id, Name, Address, IsActive));
    }

    /// <summary>
    /// Marks this cinema as deleted (soft delete handled by infrastructure).
    /// </summary>
    public void MarkAsDeleted()
    {
        RaiseEvent(new CinemaDeleted(Id, Name, Address));
    }

    // =============================================================
    // State transitions
    // =============================================================

    /// <summary>
    /// Activates the cinema for operations. No-op when already active (idempotent).
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        RaiseEvent(new CinemaActivated(Id, Name, Address));
    }

    /// <summary>
    /// Deactivates the cinema. No-op when already inactive (idempotent).
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RaiseEvent(new CinemaDeactivated(Id, Name, Address));
    }
}
