namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Publishes realtime ticket state changes to connected clients.
/// </summary>
public interface ITicketRealtimePublisher
{
    /// <summary>
    /// Pushes a ticket status delta for a specific showtime.
    /// </summary>
    Task PublishTicketStatusChangedAsync(TicketStatusChangedRealtimeEvent @event, CancellationToken ct);
}

/// <summary>
/// Delta payload emitted to clients when a single ticket status changes.
/// </summary>
public sealed record TicketStatusChangedRealtimeEvent(
    Guid ShowTimeId,
    Guid TicketId,
    string TicketCode,
    TicketStatus Status,
    DateTimeOffset OccurredAtUtc,
    string? LockingBy = null);
