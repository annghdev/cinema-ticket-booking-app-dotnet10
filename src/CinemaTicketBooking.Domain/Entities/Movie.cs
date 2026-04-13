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

    // =============================================================
    // State transitions: explicit commands per allowed transition
    // =============================================================

    /// <summary>
    /// Promotes the movie from Ongoing to NowShowing so it can be scheduled.
    /// </summary>
    public void PromoteToNowShowing()
    {
        if (Status != MovieStatus.Ongoing)
        {
            throw new InvalidOperationException(
                $"Cannot promote movie '{Name}' to NowShowing from status {Status}. Only Ongoing movies can be promoted.");
        }

        Status = MovieStatus.NowShowing;
        RaiseEvent(new MoviePromotedToNowShowing(Id, Name));
    }

    /// <summary>
    /// Marks an upcoming movie (Ongoing) as NoShow before it reaches screens.
    /// </summary>
    public void WithdrawUpcomingRunAsNoShow()
    {
        if (Status != MovieStatus.Ongoing)
        {
            throw new InvalidOperationException(
                $"Cannot withdraw upcoming run for movie '{Name}' from status {Status}. Only Ongoing movies can be withdrawn this way.");
        }

        Status = MovieStatus.NoShow;
        RaiseEvent(new MovieWithdrawnAsNoShowWhileUpcoming(Id, Name));
    }

    /// <summary>
    /// Closes a movie that is currently on screens (NowShowing) and marks it NoShow.
    /// </summary>
    public void CloseNowShowingRunAsNoShow()
    {
        if (Status != MovieStatus.NowShowing)
        {
            throw new InvalidOperationException(
                $"Cannot close now-showing run for movie '{Name}' from status {Status}. Only NowShowing movies can be closed this way.");
        }

        Status = MovieStatus.NoShow;
        RaiseEvent(new MovieRunClosedAsNoShow(Id, Name));
    }
}
