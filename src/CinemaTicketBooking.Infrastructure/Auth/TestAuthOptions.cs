namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Test-only options; never enable outside integration tests.
/// </summary>
public sealed class TestAuthOptions
{
    public const string SectionName = "TestAuth";

    /// <summary>
    /// When true, register/login responses include the raw refresh token in JSON (for Alba/TestServer clients that hide Set-Cookie).
    /// </summary>
    public bool ExposeRefreshTokenInJson { get; set; }
}
