namespace CinemaTicketBooking.Domain;

/// <summary>
/// Represents a promotional slide for the frontend homepage carousel.
/// </summary>
public class Slide : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // =============================================================
    // Factory and data mutation
    // =============================================================

    public static Slide Create(
        string title,
        string description,
        string imageUrl,
        string targetUrl,
        int displayOrder,
        bool isActive = true)
    {
        var slide = new Slide
        {
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            TargetUrl = targetUrl,
            DisplayOrder = displayOrder,
            IsActive = isActive
        };

        return slide;
    }

    public void UpdateBasicInfo(
        string title,
        string description,
        string imageUrl,
        string targetUrl,
        int displayOrder,
        bool isActive)
    {
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        TargetUrl = targetUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
}
