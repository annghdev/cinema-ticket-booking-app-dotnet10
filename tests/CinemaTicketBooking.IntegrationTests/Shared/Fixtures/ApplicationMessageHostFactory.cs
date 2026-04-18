using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.IntegrationTests.Shared.Fakes;
using CinemaTicketBooking.Infrastructure;
using CinemaTicketBooking.Infrastructure.Auth;
using CinemaTicketBooking.Infrastructure.Persistence;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public static class ApplicationMessageHostFactory
{
    public static async Task<IHost> StartAsync(string connectionString, CancellationToken ct = default)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddInfrastructure(new ConfigurationBuilder().Build());
                services.AddSingleton<IUserContext, FakeUserContext>();
                services.AddScoped<IAccountCustomerLinker, AccountCustomerLinker>();
                services.AddSingleton<InMemoryTicketRealtimePublisher>();
                services.AddSingleton<ITicketRealtimePublisher>(sp => sp.GetRequiredService<InMemoryTicketRealtimePublisher>());
                services.Configure<TicketLockingOptions>(opts =>
                {
                    opts.LockHoldSeconds = 1;
                    opts.PaymentHoldSeconds = 2;
                    opts.RecoveryBatchSize = 200;
                });
            })
            .UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(AddShowTimeCommand).Assembly);
                opts.PersistMessagesWithPostgresql(connectionString);

                opts.Services.AddDbContextWithWolverineIntegration<AppDbContext>(
                    x => x.UseNpgsql(connectionString));

                opts.Policies.AutoApplyTransactions();
                opts.AutoBuildMessageStorageOnStartup = AutoCreate.CreateOrUpdate;
            })
            .Build();

        await host.StartAsync(ct);
        return host;
    }
}
