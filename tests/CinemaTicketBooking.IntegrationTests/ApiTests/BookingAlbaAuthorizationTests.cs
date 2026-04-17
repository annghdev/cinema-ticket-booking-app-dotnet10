using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Auth;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.WebServer.ApiEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketBooking.IntegrationTests.ApiTests;

/// <summary>
/// Verifies ownership/admin authorization for booking read endpoint.
/// </summary>
[Collection(nameof(AuthAlbaCollection))]
public sealed class BookingAlbaAuthorizationTests(AuthAlbaFixture fixture)
{
    [Fact]
    public async Task Customer_Should_Access_Own_Booking_But_Forbidden_For_Other_User_Booking()
    {
        var owner = await RegisterAndLoginAsync("owner");
        var stranger = await RegisterAndLoginAsync("stranger");
        var bookingId = await SeedBookingForAccountAsync(owner.AccountId, "Owner Booking");

        using var ownerClient = fixture.CreateClient();
        SetBearer(ownerClient, owner.AccessToken);
        var ownerResponse = await ownerClient.GetAsync($"/api/bookings/{bookingId}");
        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var strangerClient = fixture.CreateClient();
        SetBearer(strangerClient, stranger.AccessToken);
        var strangerResponse = await strangerClient.GetAsync($"/api/bookings/{bookingId}");
        strangerResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Access_Other_Customer_Booking()
    {
        var owner = await RegisterAndLoginAsync("owner-admin-check");
        var bookingId = await SeedBookingForAccountAsync(owner.AccountId, "Owner Booking For Admin");

        var admin = await fixture.CreateAdminUserAsync();
        using var adminClient = fixture.CreateClient();
        var login = await adminClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(admin.Email, admin.Password));
        login.EnsureSuccessStatusCode();
        var adminToken = (await login.Content.ReadFromJsonAsync<AuthTokenApiResponse>())!.AccessToken;
        SetBearer(adminClient, adminToken);

        var response = await adminClient.GetAsync($"/api/bookings/{bookingId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<(Guid AccountId, string AccessToken)> RegisterAndLoginAsync(string prefix)
    {
        var client = fixture.CreateClient();
        var email = $"{prefix}-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, $"{prefix}-name", "0901234567"));
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthTokenApiResponse>();

        return (loginBody!.AccountId, loginBody.AccessToken);
    }

    private async Task<Guid> SeedBookingForAccountAsync(Guid accountId, string customerName)
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await db.Accounts
            .AsNoTracking()
            .SingleAsync(x => x.Id == accountId);

        account.CustomerId.Should().NotBeNull();
        var customerId = account.CustomerId!.Value;

        var cinema = IntegrationEntityBuilder.Cinema("API Booking Cinema");
        var movie = IntegrationEntityBuilder.Movie("API Booking Movie", MovieStatus.NowShowing);
        var screen = IntegrationEntityBuilder.Screen(cinema.Id, "API-SCR-1", "[[1,1,1]]");
        var showTime = IntegrationEntityBuilder.ShowTime(movie.Id, screen.Id);

        var booking = IntegrationEntityBuilder.Booking(showTime.Id, customerId, customerName);
        booking.OriginAmount = 200_000m;
        booking.FinalAmount = 180_000m;
        booking.QrCode = $"qr://api/{booking.Id}";
        booking.Status = BookingStatus.Confirmed;

        db.Cinemas.Add(cinema);
        db.Movies.Add(movie);
        db.Screens.Add(screen);
        db.ShowTimes.Add(showTime);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        return booking.Id;
    }

    private static void SetBearer(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
    }
}
