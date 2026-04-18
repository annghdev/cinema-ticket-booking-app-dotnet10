using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests;

/// <summary>
/// Wolverine probe message for <see cref="CachingMiddlewareIntegrationTests"/>.
/// </summary>
public sealed class CachingMiddlewareProbeQuery : ICachableQuery<CachingMiddlewareProbeDto>, ICachable
{
    public string CorrelationId { get; set; } = string.Empty;
    public string CacheKey { get; set; } = string.Empty;
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);
    public int Payload { get; set; }
}

public sealed record CachingMiddlewareProbeDto(int Value);

/// <summary>
/// Handler for <see cref="CachingMiddlewareProbeQuery"/>; <see cref="InvocationCount"/> tracks executions.
/// </summary>
public sealed class CachingMiddlewareProbeHandler
{
    private static int _invocationCount;

    public static int InvocationCount => _invocationCount;

    public static void ResetInvocationCount() => Interlocked.Exchange(ref _invocationCount, 0);

    public Task<CachingMiddlewareProbeDto> Handle(CachingMiddlewareProbeQuery query, CancellationToken ct)
    {
        Interlocked.Increment(ref _invocationCount);
        return Task.FromResult(new CachingMiddlewareProbeDto(query.Payload));
    }
}
