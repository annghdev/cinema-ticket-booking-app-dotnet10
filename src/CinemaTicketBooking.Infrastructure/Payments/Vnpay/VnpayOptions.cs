namespace CinemaTicketBooking.Infrastructure.Payments.Vnpay;

/// <summary>
/// Configuration for VNPay payment gateway integration.
/// </summary>
public sealed class VnpayOptions
{
    public const string SectionName = "VnPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentBaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
    public string OrderType { get; set; } = "other";
    public int ExpireMinutes { get; set; } = 15;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string IpnPath { get; set; } = "/api/payments/vnpay-ipn";
    public string FrontendResultUrl { get; set; } = "http://localhost:3000/payment-result";
}
