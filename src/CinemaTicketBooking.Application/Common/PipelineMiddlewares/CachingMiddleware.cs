using System.Collections.Concurrent;
using System.Reflection;
using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Runtime.Serialization;

namespace CinemaTicketBooking.Application.Common.PipelineMiddlewares;

/// <summary>
/// Wolverine middleware: optional response caching for messages implementing <see cref="ICachableQuery{TResponse}"/>.
/// Serialization uses <see cref="WolverineOptions.DefaultSerializer"/> (same configuration as the Wolverine runtime).
/// Cache hit: sets <see cref="Envelope.Response"/> on the active envelope (required for <c>InvokeAsync&lt;T&gt;</c> return). Cache miss: <c>Finally</c> reads <c>Envelope.Response</c> after the handler. Avoids <c>object?</c> middleware parameters that DI resolves as <see cref="object"/>.
/// </summary>
public static class CachingMiddleware
{
    private const string CachePayloadContentType = "application/json";
    private static readonly ConcurrentDictionary<Type, Type?> ResponseClrTypeByQueryType = new();
    private static readonly Lazy<PropertyInfo?> EnvelopeResponseProperty = new(() =>
        typeof(Envelope).GetProperty("Response", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

    /// <summary>
    /// Returns cached handler result and stops the chain when a cache entry exists.
    /// For <c>InvokeAsync&lt;T&gt;</c>, Wolverine returns <see cref="Envelope.Response"/> on the current envelope
    /// (<see href="https://github.com/JasperFx/wolverine/blob/main/src/Wolverine/Runtime/Handlers/Executor.cs"/>); <c>RespondToSenderAsync</c> does not set that and yields empty results on cache hit.
    /// </summary>
    public static async Task<HandlerContinuation> LoadAsync(
        ICachableQuery query,
        ICacheService cache,
        IMessageContext context,
        WolverineOptions wolverineOptions,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.CacheKey))
            return HandlerContinuation.Continue;

        var responseClrType = ResponseClrTypeByQueryType.GetOrAdd(query.GetType(), static queryType =>
        {
            foreach (var i in queryType.GetInterfaces())
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICachableQuery<>))
                    return i.GetGenericArguments()[0];
            }

            return null;
        });

        if (responseClrType is null)
        {
            logger.LogWarning(
                "Query type {QueryType} does not implement ICachableQuery<TResponse>; skipping cache read",
                query.GetType().FullName);
            return HandlerContinuation.Continue;
        }

        var payload = await cache.GetAsync<string>(query.CacheKey, cancellationToken);
        if (string.IsNullOrEmpty(payload))
            return HandlerContinuation.Continue;

        var serializer = wolverineOptions.DefaultSerializer;
        var bytes = Convert.FromBase64String(payload);
        var readEnvelope = new Envelope
        {
            Data = bytes,
            ContentType = CachePayloadContentType
        };
        readEnvelope.SetMessageType(responseClrType);

        object deserialized;
        try
        {
            deserialized = serializer.ReadFromData(responseClrType, readEnvelope);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for key {CacheKey}; ignoring cache entry", query.CacheKey);
            return HandlerContinuation.Continue;
        }

        logger.LogDebug(
            "Cache hit for {MessageType}, key {CacheKey}",
            query.GetType().FullName,
            query.CacheKey);

        if (context.Envelope is null)
        {
            logger.LogWarning("No active envelope on message context; cannot apply cache hit for {CacheKey}", query.CacheKey);
            return HandlerContinuation.Continue;
        }

        var responseProp = EnvelopeResponseProperty.Value;
        if (responseProp is null)
            return HandlerContinuation.Continue;

        responseProp.SetValue(context.Envelope, deserialized);
        return HandlerContinuation.Stop;
    }

    /// <summary>
    /// After handler + other <c>After</c> middleware; persists <see cref="Envelope.Response"/> when present.
    /// </summary>
    public static async Task Finally(
        Envelope envelope,
        ICacheService cache,
        WolverineOptions wolverineOptions,
        CancellationToken cancellationToken)
    {
        if (envelope.Message is not ICachableQuery query)
            return;

        if (string.IsNullOrWhiteSpace(query.CacheKey))
            return;

        var prop = EnvelopeResponseProperty.Value;
        if (prop is null)
            return;

        var result = prop.GetValue(envelope);
        if (result is null)
            return;

        var serializer = wolverineOptions.DefaultSerializer;
        var bytes = serializer.WriteMessage(result);
        var payload = Convert.ToBase64String(bytes);
        var sliding = query.SlidingExpiration ?? TimeSpan.FromMinutes(5);
        await cache.SetAsync(query.CacheKey, payload, sliding, cancellationToken);
    }
}
