using System.Security.Cryptography;
using System.Text;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Payments;

/// <summary>
/// Fake payment gateway for development. Always returns a success payment URL
/// with QrCode redirect behavior (simulates instant payment).
/// Uses a deterministic HMAC signature for callback verification,
/// demonstrating the same pattern real gateways must follow.
/// </summary>
public sealed class NoPaymentGatewayService : IPaymentService
{
    /// <summary>
    /// Development-only secret key for HMAC signature verification.
    /// Real implementations inject this from configuration/secrets.
    /// </summary>
    private const string SecretKey = "FAKE-GATEWAY-DEV-SECRET-KEY-DO-NOT-USE-IN-PRODUCTION";
    private const string GatewayIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" role="img" aria-label="Cash">
          <rect x="6" y="14" width="52" height="36" rx="7" fill="#0F172A"/>
          <rect x="10" y="18" width="44" height="28" rx="5" fill="#1E293B"/>
          <circle cx="32" cy="32" r="8" fill="#22D3EE"/>
          <path d="M28 32h8M32 28v8" stroke="#0F172A" stroke-width="2" stroke-linecap="round"/>
        </svg>
        """;

    public PaymentMethod Method => PaymentMethod.None;
    public PaymentRedirectBehavior RedirectBehavior => PaymentRedirectBehavior.QrCode;
    public string Icon => GatewayIcon;

    /// <summary>
    /// Creates a fake payment with a generated QR code URL.
    /// Includes a HMAC signature in the callback params for verification.
    /// </summary>
    public Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request, CancellationToken ct = default)
    {
        var fakeTransactionId = $"FAKE-{Guid.CreateVersion7()}";
        return Task.FromResult(new CreatePaymentResult(
            Success: true,
            PaymentUrl: $"https://fake-payment.local/pay/{fakeTransactionId}",
            GatewayTransactionId: fakeTransactionId,
            RedirectBehavior: PaymentRedirectBehavior.QrCode));
    }

    /// <summary>
    /// Verifies the gateway callback by validating the HMAC signature
    /// computed from the response parameters against the secret key.
    /// This mirrors the pattern used by real gateways (VnPay, Momo, etc.).
    /// </summary>
    public Task<ConfirmPaymentResult> ConfirmPaymentAsync(
        ConfirmPaymentRequest request, CancellationToken ct = default)
    {
        // 1. Extract the signature from callback params.
        if (!request.GatewayResponseParams.TryGetValue("vnp_SecureHash", out var receivedSignature)
            || string.IsNullOrEmpty(receivedSignature))
        {
            // Dev mode: if no signature params present, auto-succeed for convenience.
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: true,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId));
        }

        // 2. Build the data string from sorted params (excluding the signature itself).
        var sortedParams = request.GatewayResponseParams
            .Where(kv => kv.Key != "vnp_SecureHash")
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => $"{kv.Key}={kv.Value}");
        var rawData = string.Join("&", sortedParams);

        // 3. Compute HMAC-SHA256 with the secret key and compare.
        var expectedSignature = ComputeHmacSha256(rawData, SecretKey);
        var isValid = string.Equals(receivedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);

        // 4. Check gateway response code indicates success.
        var isGatewaySuccess = isValid
            && request.GatewayResponseParams.TryGetValue("vnp_ResponseCode", out var responseCode)
            && responseCode == "00";

        if (isGatewaySuccess)
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: true,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId));
        }

        var errorMessage = !isValid
            ? "Invalid payment signature. Possible data tampering."
            : "Payment was declined by the gateway.";

        return Task.FromResult(new ConfirmPaymentResult(
            IsSuccess: false,
            BookingId: request.BookingId,
            GatewayTransactionId: request.GatewayTransactionId,
            ErrorMessage: errorMessage));
    }

    /// <summary>
    /// Computes HMAC-SHA256 hash of the data using the provided key.
    /// </summary>
    private static string ComputeHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hash);
    }
}
