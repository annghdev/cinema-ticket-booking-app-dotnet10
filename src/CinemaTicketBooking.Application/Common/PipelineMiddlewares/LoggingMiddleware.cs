using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Application.Common.PipelineMiddlewares;

/// <summary>
/// Wolverine middleware: structured logging for message handling (duration, correlation, message name).
/// Queries: Information only when under slow threshold; Warning when slow (per logging skill).
/// </summary>
public static class LoggingMiddleware
{
    /// <summary>
    /// En Starts timing for the current message chain.
    /// </summary>
    public static Stopwatch Before()
    {
        var sw = new Stopwatch();
        sw.Start();
        return sw;
    }

    /// <summary>
    /// Emits pipeline log line after handler completion (success path; failures are logged by exception policy).
    /// </summary>
    public static void Finally(
        Stopwatch stopwatch,
        Envelope envelope,
        ILogger logger,
        IOptions<MessagePipelineLoggingOptions> options)
    {
        stopwatch.Stop();
        var durationMs = stopwatch.ElapsedMilliseconds;
        var body = envelope.Message;
        var messageName = envelope.MessageType ?? body?.GetType().FullName ?? "(unknown)";
        var correlationId = body is IRequest req ? req.CorrelationId : null;

        if (body is IQuery)
        {
            var threshold = options.Value.SlowQueryThresholdMs;
            if (durationMs >= threshold)
            {
                logger.LogWarning(
                    "Slow query message {MessageName} completed in {DurationMs} ms (threshold {ThresholdMs} ms), CorrelationId {CorrelationId}, Envelope {EnvelopeId}",
                    messageName,
                    durationMs,
                    threshold,
                    correlationId,
                    envelope.Id);
            }
            else
            {
                logger.LogDebug(
                    "Query message {MessageName} completed in {DurationMs} ms, CorrelationId {CorrelationId}, Envelope {EnvelopeId}",
                    messageName,
                    durationMs,
                    correlationId,
                    envelope.Id);
            }

            return;
        }

        logger.LogInformation(
            "Message {MessageName} handled in {DurationMs} ms, CorrelationId {CorrelationId}, Envelope {EnvelopeId}",
            messageName,
            durationMs,
            correlationId,
            envelope.Id);
    }
}
