namespace CinemaTicketBooking.Domain;

/// <summary>
/// Movie represents a film available for scheduling in the cinema system.
/// Only movies with Status = NowShowing can be assigned to a ShowTime.
/// </summary>
public class Movie : AuditableEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string? OfficialTrailerUrl { get; set; }

    /// <summary>
    /// Duration of the movie in minutes (excluding trailer time).
    /// Total screen time = Duration + ShowTime.TrailerTime.
    /// </summary>
    public int Duration { get; set; }
    public MovieGenre Genre { get; set; }
    public MovieStatus Status { get; set; }
}

//public enum MovieStatus
//{
//    Ongoing,
//    NowShowing,
//    NoShow
//}

//public enum MovieGenre
//{
//    Action,
//    Comedy,
//    Drama,
//    Horror,
//    SciFi,
//    Romance,
//    Thriller,
//    Animation,
//    Documentary
//}