namespace CinemaTicketBooking.Domain;

public class ShowTime : AuditableEntity
{
    public Guid MovieId { get; set; }
    public Movie? Movie { get; set; }
    public Guid ScreenId { get; set; }
    public Screen? Screen { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public ShowTimeStatus Status { get; set; }
    public List<Ticket> Tickets { get; set; } = [];

    // Configuration constants
    public static readonly TimeSpan CleanupBuffer = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan TrailerTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The actual time this ShowTime occupies the Screen (including cleanup buffer).
    /// Used for conflict detection.
    /// </summary>
    public DateTimeOffset OccupiedUntil => EndAt.Add(CleanupBuffer);

    // =============================================================
    // Factory Method: Ensures all invariants are satisfied at creation
    // =============================================================
    public static ShowTime Create(
        Movie movie, 
        Screen screen, 
        DateTimeOffset startAt, 
        List<PricingPolicy> pricingPolicies)
    {
        // 1. Validate Movie status
        if (movie.Status != MovieStatus.NowShowing)
            throw new InvalidOperationException(
                $"Movie '{movie.Name}' is not available for scheduling (Status: {movie.Status}).");

        // 2. Validate Screen is active
        if (!screen.IsActive)
            throw new InvalidOperationException(
                $"Screen '{screen.Code}' is not active.");

        // 3. Validate future time
        if (startAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException(
                "ShowTime must be scheduled in the future.");

        // 4. Validate Screen has seats
        if (screen.Seats is null || screen.Seats.Count == 0)
            throw new InvalidOperationException(
                $"Screen '{screen.Code}' has no seats. Cannot create a showtime.");

        // 5. Validate pricing policies cover all seat types in the screen
        if (pricingPolicies is null || pricingPolicies.Count == 0)
            throw new InvalidOperationException(
                "Pricing policies are required to create a showtime.");

        // 6. Calculate EndAt = StartAt + TrailerTime + Movie Duration
        var endAt = startAt
            .Add(TrailerTime)
            .Add(TimeSpan.FromMinutes(movie.Duration));

        // 7. Create the ShowTime entity
        var showTime = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            MovieId = movie.Id,
            Movie = movie,
            ScreenId = screen.Id,
            Screen = screen,
            Date = DateOnly.FromDateTime(startAt.DateTime),
            StartAt = startAt,
            EndAt = endAt,
            Status = ShowTimeStatus.Ongoing
        };

        // 8. Generate tickets with pricing for every active seat in the screen
        showTime.GenerateTickets(screen, pricingPolicies);

        // 9. Raise domain event
        showTime.RaiseEvent(new ShowTimeCreated(
            ShowTimeId: showTime.Id,
            MovieId: movie.Id,
            MovieName: movie.Name,
            ScreenId: screen.Id,
            ScreenCode: screen.Code,
            Date: showTime.Date,
            StartAt: showTime.StartAt,
            EndAt: showTime.EndAt,
            TicketCount: showTime.Tickets.Count));

        return showTime;
    }

    // =============================================================
    // Generate Tickets from Screen's Seat list with pricing
    // =============================================================
    private void GenerateTickets(Screen screen, List<PricingPolicy> pricingPolicies)
    {
        foreach (var seat in screen.Seats.Where(s => s.IsActive))
        {
            var policy = pricingPolicies
                .FirstOrDefault(p => p.SeatType == seat.Type && p.IsActive);

            if (policy is null)
                throw new InvalidOperationException(
                    $"No pricing policy found for ScreenType '{screen.Type}', SeatType '{seat.Type}'.");

            Tickets.Add(new Ticket
            {
                Id = Guid.CreateVersion7(),
                Code = $"{Date:yyyyMMdd}-{screen.Code}-{seat.Code}",
                Description = $"{seat.Code} - {seat.Type}",
                Price = policy.CalculatePrice(),
                ShowTimeId = Id,
                Status = TicketStatus.Available
            });
        }
    }

    // =============================================================
    // Conflict detection: checks if this ShowTime overlaps another
    // on the same Screen (including cleanup buffer)
    // =============================================================
    public bool ConflictsWith(ShowTime other)
    {
        if (ScreenId != other.ScreenId) return false;
        if (other.Status == ShowTimeStatus.Cancelled) return false;

        // Two intervals [StartAt, OccupiedUntil) overlap when:
        // this.Start < other.End AND other.Start < this.End
        return StartAt < other.OccupiedUntil && other.StartAt < OccupiedUntil;
    }

    // =============================================================
    // State Transitions
    // =============================================================
    public void StartShowing()
    {
        if (Status != ShowTimeStatus.Ongoing)
            throw new InvalidOperationException(
                "Only ongoing showtimes can start showing.");
        Status = ShowTimeStatus.Showing;

        RaiseEvent(new ShowTimeStarted(
            ShowTimeId: Id,
            MovieId: MovieId,
            ScreenId: ScreenId,
            StartAt: StartAt));
    }

    public void Complete()
    {
        if (Status != ShowTimeStatus.Showing)
            throw new InvalidOperationException(
                "Only showing showtimes can be completed.");
        Status = ShowTimeStatus.Completed;

        RaiseEvent(new ShowTimeCompleted(
            ShowTimeId: Id,
            MovieId: MovieId,
            ScreenId: ScreenId,
            StartAt: StartAt,
            EndAt: EndAt));
    }

    public void Cancel()
    {
        if (Status is ShowTimeStatus.Showing or ShowTimeStatus.Completed)
            throw new InvalidOperationException(
                "Cannot cancel a showtime that is showing or already completed.");

        Status = ShowTimeStatus.Cancelled;

        RaiseEvent(new ShowTimeCancelled(
            ShowTimeId: Id,
            MovieId: MovieId,
            MovieName: Movie?.Name ?? string.Empty,
            ScreenId: ScreenId,
            ScreenCode: Screen?.Code ?? string.Empty,
            Date: Date,
            StartAt: StartAt));
    }
}
