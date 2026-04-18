namespace CinemaTicketBooking.Application.Common.PipelineMiddlewares;

/// <summary>
/// En Wolverine message pipeline logging thresholds (queries: warn only when slow).
/// </summary>
public sealed class MessagePipelineLoggingOptions
{
    public const string SectionName = "MessagePipelineLogging";

    /// <summary>
    /// En When duration exceeds this value, <see cref="IQuery"/> messages are logged at Warning.
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 500;
}
