namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Configures ticket locking, pending-payment timeout, and recovery behavior.
/// </summary>
public class TicketLockingOptions
{
    public const string SectionName = "TicketLocking";

    /// <summary>
    /// Locking-stage hold timeout in seconds before auto-release is triggered.
    /// </summary>
    public int LockHoldSeconds { get; set; } = 300;

    /// <summary>
    /// Pending-payment hold timeout in seconds before auto-release is triggered.
    /// </summary>
    public int PaymentHoldSeconds { get; set; } = 900;

    /// <summary>
    /// Maximum number of stale locks to recover in one startup run.
    /// </summary>
    public int RecoveryBatchSize { get; set; } = 200;

    public TimeSpan LockHoldDuration => TimeSpan.FromSeconds(LockHoldSeconds);
    public TimeSpan PaymentHoldDuration => TimeSpan.FromSeconds(PaymentHoldSeconds);
}
