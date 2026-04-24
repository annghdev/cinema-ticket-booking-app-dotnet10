namespace CinemaTicketBooking.Infrastructure.Payments.Momo;

/// <summary>
/// Configuration for MoMo payment integration.
/// </summary>
public sealed class MomoOptions
{
    public const string SectionName = "Momo";

    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string CreateEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
    public string RequestType { get; set; } = "captureWallet";
    public string Language { get; set; } = "vi";
    public int TimeoutSeconds { get; set; } = 15;

    public string PublicBaseUrl { get; set; } = string.Empty;
    public string IpnPath { get; set; } = "/api/payments/momo-ipn";
    public string ReturnPath { get; set; } = "/api/payments/momo-return";
    public string FrontendResultUrl { get; set; } = "http://localhost:5173/payment-result";
}
