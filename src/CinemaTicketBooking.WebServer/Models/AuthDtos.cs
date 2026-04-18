namespace CinemaTicketBooking.WebServer.ApiEndpoints;

public sealed record RegisterRequest(string Email, string Password, string Name, string PhoneNumber);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthTokenApiResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    Guid AccountId,
    string? RefreshToken = null);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(Guid UserId, string Code, string NewPassword);

public sealed record DeleteAccountRequest(string Password);

public sealed record LockAccountRequest(DateTimeOffset? LockoutEndUtc);
