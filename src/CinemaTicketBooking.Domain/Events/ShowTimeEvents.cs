namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a new ShowTime is created via the factory method.
/// Side effects: notify admins, cache warm-up, schedule auto-cancel cron job.
/// </summary>
public record ShowTimeCreated(
    Guid ShowTimeId,
    Guid MovieId,
    string MovieName,
    Guid ScreenId,
    string ScreenCode,
    DateOnly Date,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int TicketCount) : IDomainEvent;

/// <summary>
/// Raised when a ShowTime is cancelled.
/// Side effects: cancel all related bookings, notify affected customers, process refunds.
/// </summary>
public record ShowTimeCancelled(
    Guid ShowTimeId,
    Guid MovieId,
    string MovieName,
    Guid ScreenId,
    string ScreenCode,
    DateOnly Date,
    DateTimeOffset StartAt) : IDomainEvent;

/// <summary>
/// Raised when a ShowTime transitions from Ongoing → Showing.
/// Side effects: lock remaining available tickets, send "now showing" analytics.
/// </summary>
public record ShowTimeStarted(
    Guid ShowTimeId,
    Guid MovieId,
    Guid ScreenId,
    DateTimeOffset StartAt) : IDomainEvent;

/// <summary>
/// Raised when a ShowTime transitions from Showing → Completed.
/// Side effects: cleanup resources, generate revenue report, release screen slot.
/// </summary>
public record ShowTimeCompleted(
    Guid ShowTimeId,
    Guid MovieId,
    Guid ScreenId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt) : IDomainEvent;