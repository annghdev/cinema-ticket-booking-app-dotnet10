using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CinemaTicketBooking.WebServer.ApiEndpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CinemaTicketBooking.IntegrationTests.ApiTests;

/// <summary>
/// Alba smoke tests for JWT, refresh cookies, policy-based admin routes, and logout revocation.
/// </summary>
[Collection(nameof(AuthAlbaCollection))]
public sealed class AuthAlbaTests(AuthAlbaFixture fixture)
{
    private const string RefreshCookieName = "rt";

    [Fact]
    public async Task Register_Or_Login_Should_Return_AccessToken()
    {
        var client = fixture.CreateClient();
        var email = $"user-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";

        var reg = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Alba User", "0900000001"));
        reg.StatusCode.Should().Be(HttpStatusCode.OK);

        var regBody = await reg.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        regBody.Should().NotBeNull();
        regBody!.AccessToken.Should().NotBeNullOrWhiteSpace();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await login.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_Should_Issue_New_AccessToken_When_Cookie_Present()
    {
        var client = fixture.CreateClient();
        var email = $"user-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";

        var reg = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Refresh User", "0900000002"));
        reg.EnsureSuccessStatusCode();
        var regBody = await reg.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        regBody!.RefreshToken.Should().NotBeNullOrWhiteSpace();

        using var refreshReq = RefreshRequest(regBody.RefreshToken!);
        var refresh = await client.SendAsync(refreshReq);
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await refresh.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Customer_Should_Be_Forbidden_From_Admin_Lock_Endpoint()
    {
        var client = fixture.CreateClient();
        var email = $"cust-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";
        await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Customer", "0900000003"));

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<AuthTokenApiResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

        var victim = Guid.CreateVersion7();
        var lockResponse = await client.PostAsJsonAsync($"/api/auth/admin/accounts/{victim}/lock", new LockAccountRequest(null));
        lockResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Lock_Target_Account_And_Victim_Cannot_Login()
    {
        var adminClient = fixture.CreateClient();
        var admin = await fixture.CreateAdminUserAsync();
        var adminLogin = await adminClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(admin.Email, admin.Password));
        adminLogin.EnsureSuccessStatusCode();
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<AuthTokenApiResponse>())!.AccessToken;
        adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, adminToken);

        var victimClient = fixture.CreateClient();
        var victimEmail = $"victim-{Guid.CreateVersion7():N}@test.local";
        const string victimPassword = "Aa123456!";
        var victimReg = await victimClient.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(victimEmail, victimPassword, "Victim", "0900000004"));
        victimReg.EnsureSuccessStatusCode();
        var victimId = (await victimReg.Content.ReadFromJsonAsync<AuthTokenApiResponse>())!.AccountId;

        var lockResp = await adminClient.PostAsJsonAsync(
            $"/api/auth/admin/accounts/{victimId}/lock",
            new LockAccountRequest(DateTimeOffset.UtcNow.AddMinutes(30)));
        lockResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var victimLoginAfter = await victimClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(victimEmail, victimPassword));
        victimLoginAfter.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Should_Invalidate_Refresh_Session()
    {
        var client = fixture.CreateClient();
        var email = $"out-{Guid.CreateVersion7():N}@test.local";
        const string password = "Aa123456!";
        var reg = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Logout User", "0900000005"));
        reg.EnsureSuccessStatusCode();
        var regBody = await reg.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        regBody!.RefreshToken.Should().NotBeNullOrWhiteSpace();

        using var refreshOkReq = RefreshRequest(regBody.RefreshToken!);
        var refreshOk = await client.SendAsync(refreshOkReq);
        refreshOk.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = await refreshOk.Content.ReadFromJsonAsync<AuthTokenApiResponse>();
        rotated!.RefreshToken.Should().NotBeNullOrWhiteSpace();

        using var logoutReq = LogoutRequest(rotated.RefreshToken!);
        var logout = await client.SendAsync(logoutReq);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var refreshFailReq = RefreshRequest(rotated.RefreshToken!);
        var refreshFail = await client.SendAsync(refreshFailReq);
        refreshFail.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static HttpRequestMessage RefreshRequest(string rawRefreshToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        msg.Headers.TryAddWithoutValidation("Cookie", $"{RefreshCookieName}={rawRefreshToken}");
        return msg;
    }

    private static HttpRequestMessage LogoutRequest(string rawRefreshToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        msg.Headers.TryAddWithoutValidation("Cookie", $"{RefreshCookieName}={rawRefreshToken}");
        return msg;
    }
}
