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
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:redis"] = string.Empty,
                ["Momo:PartnerCode"] = "MOMO_TEST_PARTNER",
                ["Momo:AccessKey"] = "MOMO_TEST_ACCESS_KEY",
                ["Momo:SecretKey"] = "MOMO_TEST_SECRET_KEY",
                ["Momo:CreateEndpoint"] = "https://test-payment.momo.vn/v2/gateway/api/create",
                ["Momo:PublicBaseUrl"] = "http://localhost",
                ["VnPay:TmnCode"] = "TEST12345",
                ["VnPay:HashSecret"] = "TEST_VNPAY_SECRET_KEY"
            })
            .Build();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddInfrastructure(configuration);
                services.AddSingleton<IUserContext, FakeUserContext>();
                services.AddScoped<IAccountCustomerLinker, AccountCustomerLinker>();
                services.AddSingleton<InMemoryTicketRealtimePublisher>();
                services.AddSingleton<ITicketRealtimePublisher>(sp => sp.GetRequiredService<InMemoryTicketRealtimePublisher>());
                services.AddSingleton<InMemoryPaymentRealtimePublisher>();
                services.AddSingleton<IPaymentRealtimePublisher>(sp => sp.GetRequiredService<InMemoryPaymentRealtimePublisher>());
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
