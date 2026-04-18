using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Common.PipelineMiddlewares;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.IntegrationTests.Shared.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests;

/// <summary>
/// Minimal Wolverine host for <see cref="CachingMiddleware"/> integration tests (no database).
/// </summary>
public static class CachingMiddlewareTestHostFactory
{
    public static async Task<IHost> StartAsync(CancellationToken ct = default)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => { services.AddSingleton<ICacheService, InMemoryCacheService>(); })
            .UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(IRequest).Assembly);
                opts.Discovery.IncludeAssembly(typeof(CachingMiddlewareProbeQuery).Assembly);
                opts.Policies.ForMessagesOfType<ICachableQuery>().AddMiddleware(typeof(CachingMiddleware));
            })
            .Build();

        await host.StartAsync(ct);
        return host;
    }
}
