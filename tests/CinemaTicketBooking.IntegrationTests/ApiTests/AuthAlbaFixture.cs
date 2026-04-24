using Alba;
using CinemaTicketBooking.Application.Common.Auth;
using CinemaTicketBooking.Infrastructure.Auth;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace CinemaTicketBooking.IntegrationTests.ApiTests;

/// <summary>
/// Shared PostgreSQL + Alba host for HTTP auth integration tests.
/// </summary>
public sealed class AuthAlbaFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("cinema_auth_alba")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    public IAlbaHost Host { get; private set; } = null!;

    /// <summary>
    /// HttpClient that retains cookies between requests (required for refresh token cookie).
    /// </summary>
    public HttpClient CreateClient()
    {
        var client = Host.Server.CreateClient();
        client.BaseAddress = new Uri("http://localhost");
        return client;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Host = await AlbaHost.For<Program>(builder =>
        {
            builder.UseSetting("ConnectionStrings:cinemadb", _postgres.GetConnectionString());
            builder.UseSetting("ConnectionStrings:redis", string.Empty);
            builder.UseSetting($"Jwt:{nameof(JwtOptions.Issuer)}", "CinemaTicketBookingTests");
            builder.UseSetting($"Jwt:{nameof(JwtOptions.Audience)}", "CinemaTicketBookingTests");
            builder.UseSetting($"Jwt:{nameof(JwtOptions.SigningKey)}", "Alba_IntegrationTest_Signing_Key_32__!");
            builder.UseSetting($"Jwt:{nameof(JwtOptions.AccessTokenMinutes)}", "15");
            builder.UseSetting($"RefreshToken:{nameof(RefreshTokenOptions.CookieName)}", "rt");
            builder.UseSetting($"RefreshToken:{nameof(RefreshTokenOptions.CookieSecure)}", "false");
            builder.UseSetting($"RefreshToken:{nameof(RefreshTokenOptions.CookiePath)}", "/");
            builder.UseSetting($"RefreshToken:{nameof(RefreshTokenOptions.CookieSameSite)}", "Lax");
            builder.UseSetting($"RefreshToken:{nameof(RefreshTokenOptions.DaysValid)}", "14");
            builder.UseSetting($"TestAuth:{nameof(TestAuthOptions.ExposeRefreshTokenInJson)}", "true");
            builder.UseSetting("VnPay:TmnCode", "TEST12345");
            builder.UseSetting("VnPay:HashSecret", "TEST_VNPAY_SECRET_KEY");
            builder.UseSetting("VnPay:PaymentBaseUrl", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
            builder.UseSetting("VnPay:FrontendResultUrl", "http://localhost:3000/payment-result");
            builder.UseSetting("Momo:PartnerCode", "MOMO_TEST_PARTNER");
            builder.UseSetting("Momo:AccessKey", "MOMO_TEST_ACCESS_KEY");
            builder.UseSetting("Momo:SecretKey", "MOMO_TEST_SECRET_KEY");
            builder.UseSetting("Momo:CreateEndpoint", "https://test-payment.momo.vn/v2/gateway/api/create");
            builder.UseSetting("Momo:PublicBaseUrl", "http://localhost");
            builder.UseSetting("Momo:FrontendResultUrl", "http://localhost:3000/payment-result");
            builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
        });
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Creates an admin account directly via Identity (faster than exercising external OAuth).
    /// </summary>
    public async Task<(string Email, string Password)> CreateAdminUserAsync()
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthAlbaFixture");
        await IdentityDataSeeder.SeedAsync(roles, logger);

        var email = $"admin-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";
        var admin = new Account
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var create = await users.CreateAsync(admin, password);
        if (!create.Succeeded)
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));

        var addRole = await users.AddToRoleAsync(admin, RoleNames.Admin);
        if (!addRole.Succeeded)
            throw new InvalidOperationException(string.Join("; ", addRole.Errors.Select(e => e.Description)));

        return (email, password);
    }
}
