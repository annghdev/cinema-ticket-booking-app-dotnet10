using CinemaTicketBooking.Application;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.FeatureTests;

[Collection(DatabaseCollection.Name)]
public abstract class ApplicationFeatureTestBase(PostgresContainerFixture databaseFixture) : IAsyncLifetime
{
    private IHost? _messageHost;

    protected PostgresContainerFixture DatabaseFixture { get; } = databaseFixture;

    protected IMessageBus MessageBus =>
        _messageHost?.Services.GetRequiredService<IMessageBus>()
        ?? throw new InvalidOperationException("Wolverine message host has not been initialized.");

    public virtual async Task InitializeAsync()
    {
        _messageHost = await ApplicationMessageHostFactory.StartAsync(DatabaseFixture.ConnectionString);
    }

    public virtual async Task DisposeAsync()
    {
        if (_messageHost is not null)
        {
            await _messageHost.StopAsync();
            _messageHost.Dispose();
        }
    }

    protected Task ResetDatabaseAsync(CancellationToken ct = default)
    {
        return DatabaseFixture.ResetDatabaseAsync(ct);
    }

    protected AppDbContext CreateDbContext()
    {
        return DatabaseFixture.CreateDbContext();
    }

    protected FakeUserContext FakeUserContext
    {
        get
        {
            var userContext = _messageHost?.Services.GetRequiredService<IUserContext>()
                ?? throw new InvalidOperationException("Wolverine message host has not been initialized.");

            return userContext as FakeUserContext
                ?? throw new InvalidOperationException("Configured IUserContext is not FakeUserContext.");
        }
    }

    protected async Task<TResponse> InvokeAsync<TResponse>(object request, CancellationToken ct = default)
    {
        return await MessageBus.InvokeAsync<TResponse>(request, ct);
    }

    protected async Task InvokeAsync(object request, CancellationToken ct = default)
    {
        await MessageBus.InvokeAsync(request, ct);
    }

    protected T ResolveRequiredService<T>() where T : notnull
    {
        if (_messageHost is null)
        {
            throw new InvalidOperationException("Wolverine message host has not been initialized.");
        }

        return _messageHost.Services.GetRequiredService<T>();
    }
}
