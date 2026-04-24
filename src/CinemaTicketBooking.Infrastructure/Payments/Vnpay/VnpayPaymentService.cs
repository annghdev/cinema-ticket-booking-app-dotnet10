using System.Globalization;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Infrastructure.Payments.Vnpay;

/// <summary>
/// VNPay gateway implementation for building payment URL and verifying callback signature.
/// </summary>
public sealed class VnpayPaymentService(IOptions<VnpayOptions> options) : IPaymentService
{
    private readonly VnpayOptions _options = options.Value;
    private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    private const string GatewayIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" role="img" aria-label="VNPay">
          <rect width="64" height="64" rx="14" fill="#F8FAFC"/>
          <path d="M10 22h10l6 20h-8l-8-20Zm14 0h8l3 10 3-10h8l-7 20h-8l-7-20Zm24 0h6v20h-6V22Z" fill="#0F4C81"/>
        </svg>
        """;

    public PaymentMethod Method => PaymentMethod.VnPay;
    public PaymentRedirectBehavior RedirectBehavior => PaymentRedirectBehavior.Redirect;
    public string Icon => GatewayIcon;

    /// <summary>
    /// Creates a signed VNPay payment URL.
    /// </summary>
    public Task<CreatePaymentResult> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        // 1. Validate gateway configuration.
        if (string.IsNullOrWhiteSpace(_options.TmnCode)
            || string.IsNullOrWhiteSpace(_options.HashSecret)
            || string.IsNullOrWhiteSpace(_options.PaymentBaseUrl))
        {
            return Task.FromResult(new CreatePaymentResult(
                Success: false,
                PaymentUrl: string.Empty,
                GatewayTransactionId: string.Empty,
                RedirectBehavior: RedirectBehavior,
                ErrorMessage: "VNPay is not configured."));
        }

        // 2. Build VNPay parameters.
        var txnRef = BuildTxnRef(request.BookingId);
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTimeZone);
        var expireAt = now.AddMinutes(Math.Max(_options.ExpireMinutes, 1));
        var amount = Convert.ToInt64(decimal.Round(request.Amount * 100m, 0, MidpointRounding.AwayFromZero));

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = _options.Command,
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = amount.ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = expireAt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = _options.CurrCode,
            ["vnp_IpAddr"] = request.IpAddress,
            ["vnp_Locale"] = _options.Locale,
            ["vnp_OrderInfo"] = request.OrderDescription,
            ["vnp_OrderType"] = _options.OrderType,
            ["vnp_ReturnUrl"] = request.ReturnUrl,
            ["vnp_TxnRef"] = txnRef
        };

        // 3. Sign and compose payment URL.
        var canonicalData = VnpayHashing.BuildCanonicalQuery(parameters);
        var secureHash = VnpayHashing.ComputeSignature(canonicalData, _options.HashSecret);
        var paymentUrl = $"{_options.PaymentBaseUrl}?{canonicalData}&vnp_SecureHash={secureHash}";

        return Task.FromResult(new CreatePaymentResult(
            Success: true,
            PaymentUrl: paymentUrl,
            GatewayTransactionId: txnRef,
            RedirectBehavior: RedirectBehavior));
    }

    /// <summary>
    /// Verifies VNPay callback signature and payment status.
    /// </summary>
    public Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.HashSecret))
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "VNPay hash secret is missing."));
        }

        // 1. Check secure hash exists.
        if (!request.GatewayResponseParams.TryGetValue("vnp_SecureHash", out var receivedSignature)
            || string.IsNullOrWhiteSpace(receivedSignature))
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "Invalid payment signature."));
        }

        // 2. Recompute signature from returned parameters.
        var canonicalData = VnpayHashing.BuildCanonicalQuery(request.GatewayResponseParams
            .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType"));
        var expectedSignature = VnpayHashing.ComputeSignature(canonicalData, _options.HashSecret);
        var isSignatureValid = string.Equals(expectedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);
        if (!isSignatureValid)
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "Invalid payment signature."));
        }

        // 3. Validate VNPay response status.
        request.GatewayResponseParams.TryGetValue("vnp_ResponseCode", out var responseCode);
        request.GatewayResponseParams.TryGetValue("vnp_TransactionStatus", out var transactionStatus);
        var isSuccess = string.Equals(responseCode, "00", StringComparison.Ordinal)
                        && (string.IsNullOrWhiteSpace(transactionStatus)
                            || string.Equals(transactionStatus, "00", StringComparison.Ordinal));

        var txnRef = request.GatewayResponseParams.TryGetValue("vnp_TxnRef", out var receivedTxnRef)
            ? receivedTxnRef
            : request.GatewayTransactionId;

        if (!isSuccess)
        {
            var failureCode = string.IsNullOrWhiteSpace(transactionStatus) ? responseCode : transactionStatus;
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: txnRef,
                ErrorMessage: $"Payment declined by VNPay ({failureCode ?? "unknown"})."));
        }

        return Task.FromResult(new ConfirmPaymentResult(
            IsSuccess: true,
            BookingId: request.BookingId,
            GatewayTransactionId: txnRef));
    }

    private static string BuildTxnRef(Guid bookingId)
    {
        return bookingId.ToString("N", CultureInfo.InvariantCulture).ToUpperInvariant();
    }
}
