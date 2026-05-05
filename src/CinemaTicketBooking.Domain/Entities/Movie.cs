namespace CinemaTicketBooking.Domain;

/// <summary>
/// Movie represents a film available for scheduling in the cinema system.
/// Only movies with Status = NowShowing can be assigned to a ShowTime.
/// </summary>
public class Movie : AggregateRoot
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

    /// <summary>
    /// Revenue target goal for this movie (e.g. 1,500,000,000 VND).
    /// Used for dashboard performance tracking.
    /// </summary>
    public decimal TargetReach { get; set; }

    // =============================================================
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new movie and raises a creation event.
    /// </summary>
    public static Movie Create(
        string name,
        string description,
        string thumbnailUrl,
        string studio,
        string director,
        string? officialTrailerUrl,
        int duration,
        MovieGenre genre,
        MovieStatus status,
        decimal targetReach = 0m)
    {
        var movie = new Movie
        {
            Name = name,
            Description = description,
            ThumbnailUrl = thumbnailUrl,
            Studio = studio,
            Director = director,
            OfficialTrailerUrl = officialTrailerUrl,
            Duration = duration,
            Genre = genre,
            Status = status,
            TargetReach = targetReach
        };

        movie.RaiseEvent(new MovieCreated(movie.Id, movie.Name, movie.Status, movie.Genre));
        return movie;
    }

    /// <summary>
    /// Updates basic movie metadata and raises an update event.
    /// </summary>
    public void UpdateBasicInfo(
        string name,
        string description,
        string thumbnailUrl,
        string studio,
        string director,
        string? officialTrailerUrl,
        int duration,
        MovieGenre genre,
        decimal targetReach = 0m)
    {
        Name = name;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        Studio = studio;
        Director = director;
        OfficialTrailerUrl = officialTrailerUrl;
        Duration = duration;
        Genre = genre;
        TargetReach = targetReach;

        RaiseEvent(new MovieBasicInfoUpdated(Id, Name, Status, Genre));
    }

    /// <summary>
    /// Marks this movie as deleted (soft delete handled by infrastructure).
    /// </summary>
    public void MarkAsDeleted()
    {
        RaiseEvent(new MovieDeleted(Id, Name));
    }

    // =============================================================
    // State Transitions
    // =============================================================

    /// <summary>
    /// Promotes the movie from Ongoing to NowShowing so it can be scheduled.
    /// </summary>
    public void PromoteToNowShowing()
    {
        if (Status != MovieStatus.Upcoming)
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
        if (Status != MovieStatus.Upcoming)
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
