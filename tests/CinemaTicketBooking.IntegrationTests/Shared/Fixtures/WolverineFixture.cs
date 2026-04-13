using CinemaTicketBooking.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public sealed class WolverineFixture : IAsyncLifetime
{
    private IHost? _host;

    public TestDomainEventHandlerProbe Probe =>
        _host?.Services.GetRequiredService<TestDomainEventHandlerProbe>()
        ?? throw new InvalidOperationException("Wolverine host has not been initialized.");

    public IMessageBus MessageBus =>
        _host?.Services.GetRequiredService<IMessageBus>()
        ?? throw new InvalidOperationException("Wolverine host has not been initialized.");

    public async Task InitializeAsync()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<TestDomainEventHandlerProbe>();
            })
            .UseWolverine(options =>
            {
                options.Discovery.IncludeAssembly(typeof(MoviePromotedToNowShowingHandler).Assembly);
                options.LocalQueue("domain_events");
                options.PublishMessage<MoviePromotedToNowShowing>().ToLocalQueue("domain_events");
            })
            .Build();

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
