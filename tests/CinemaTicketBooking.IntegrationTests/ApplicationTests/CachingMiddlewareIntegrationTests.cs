using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.IntegrationTests.Shared.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests;

public sealed class CachingMiddlewareIntegrationTests : IAsyncLifetime
{
    private IHost? _host;

    private IMessageBus MessageBus =>
        _host?.Services.GetRequiredService<IMessageBus>()
        ?? throw new InvalidOperationException("Host not started.");

    private ICacheService Cache =>
        _host?.Services.GetRequiredService<ICacheService>()
        ?? throw new InvalidOperationException("Host not started.");

    public async Task InitializeAsync()
    {
        _host = await CachingMiddlewareTestHostFactory.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    [Fact]
    public async Task Empty_cache_key_runs_handler_every_invoke()
    {
        CachingMiddlewareProbeHandler.ResetInvocationCount();
        await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = "",
            Payload = 1
        });
        await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = "",
            Payload = 2
        });

        CachingMiddlewareProbeHandler.InvocationCount.Should().Be(2);
    }

    [Fact]
    public async Task Same_cache_key_second_invoke_returns_cached_payload_without_re_running_handler()
    {
        CachingMiddlewareProbeHandler.ResetInvocationCount();
        await Cache.ClearAsync();

        var cacheKey = "it-cache-" + Guid.CreateVersion7();
        var first = await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = cacheKey,
            Payload = 42
        });

        (await Cache.GetAsync<string>(cacheKey)).Should().NotBeNullOrEmpty("cache should be written after first invoke");

        var second = await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = cacheKey,
            Payload = 99
        });

        first.Value.Should().Be(42);
        second.Value.Should().Be(42);
        CachingMiddlewareProbeHandler.InvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task Different_cache_keys_invoke_handler_separately()
    {
        CachingMiddlewareProbeHandler.ResetInvocationCount();
        await Cache.ClearAsync();

        await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = "it-a-" + Guid.CreateVersion7(),
            Payload = 1
        });
        await MessageBus.InvokeAsync<CachingMiddlewareProbeDto>(new CachingMiddlewareProbeQuery
        {
            CorrelationId = "caching-it",
            CacheKey = "it-b-" + Guid.CreateVersion7(),
            Payload = 2
        });

        CachingMiddlewareProbeHandler.InvocationCount.Should().Be(2);
    }
}
