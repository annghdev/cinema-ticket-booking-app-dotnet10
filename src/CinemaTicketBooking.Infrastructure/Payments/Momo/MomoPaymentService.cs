using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaTicketBooking.Infrastructure.Payments.Momo;

/// <summary>
/// MoMo QR-only gateway implementation.
/// </summary>
public sealed class MomoPaymentService(
    IOptions<MomoOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<MomoPaymentService> logger) : IPaymentService
{
    private readonly MomoOptions _options = options.Value;
    private const string GatewayIcon = "/assets/momo-logo.png";

    public PaymentMethod Method => PaymentMethod.Momo;
    //public PaymentRedirectBehavior RedirectBehavior => PaymentRedirectBehavior.Redirect;
    public PaymentRedirectBehavior RedirectBehavior => PaymentRedirectBehavior.Redirect;
    public string Icon => GatewayIcon;

    /// <summary>
    /// Creates payment request on MoMo create API and returns pay/deeplink URL.
    /// </summary>
    public async Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            return new CreatePaymentResult(
                Success: false,
                PaymentUrl: string.Empty,
                GatewayTransactionId: string.Empty,
                RedirectBehavior: RedirectBehavior,
                ErrorMessage: "MoMo is not configured.");
        }

        // 1. Build request payload.
        var orderId = BuildOrderId(request.PaymentTransactionId);
        var requestId = $"MOMO-{Guid.CreateVersion7():N}";
        var amount = Convert.ToInt64(decimal.Round(request.Amount, 0, MidpointRounding.AwayFromZero))
            .ToString(CultureInfo.InvariantCulture);
        var ipnUrl = BuildPublicUrl(_options.IpnPath);
        var redirectUrl = string.IsNullOrWhiteSpace(request.ReturnUrl)
            ? _options.FrontendResultUrl
            : request.ReturnUrl;
        var extraData = string.Empty;

        var signingFields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["accessKey"] = _options.AccessKey,
            ["amount"] = amount,
            ["extraData"] = extraData,
            ["ipnUrl"] = ipnUrl ?? string.Empty,
            ["orderId"] = orderId,
            ["orderInfo"] = request.OrderDescription,
            ["partnerCode"] = _options.PartnerCode,
            ["redirectUrl"] = redirectUrl,
            ["requestId"] = requestId,
            ["requestType"] = _options.RequestType
        };

        var rawSignature = MomoSigning.BuildRawString(signingFields,
        [
            "accessKey",
            "amount",
            "extraData",
            "ipnUrl",
            "orderId",
            "orderInfo",
            "partnerCode",
            "redirectUrl",
            "requestId",
            "requestType"
        ]);
        var signature = MomoSigning.ComputeHmacSha256(rawSignature, _options.SecretKey);

        var payload = new MomoCreatePaymentRequest(
            PartnerCode: _options.PartnerCode,
            AccessKey: _options.AccessKey,
            RequestId: requestId,
            Amount: amount,
            OrderId: orderId,
            OrderInfo: request.OrderDescription,
            RedirectUrl: redirectUrl,
            IpnUrl: ipnUrl ?? string.Empty,
            RequestType: _options.RequestType,
            ExtraData: extraData,
            Lang: _options.Language,
            Signature: signature);

        // 2. Call MoMo create endpoint.
        logger.LogInformation("[MomoPayment] Creating payment: OrderId={OrderId}, IpnUrl={IpnUrl}, RedirectUrl={RedirectUrl}",
            orderId, ipnUrl, redirectUrl);

        using var client = httpClientFactory.CreateClient(nameof(MomoPaymentService));
        client.Timeout = TimeSpan.FromSeconds(Math.Max(_options.TimeoutSeconds, 5));
        using var response = await client.PostAsJsonAsync(_options.CreateEndpoint, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new CreatePaymentResult(
                Success: false,
                PaymentUrl: string.Empty,
                GatewayTransactionId: orderId,
                RedirectBehavior: RedirectBehavior,
                ErrorMessage: $"MoMo create payment failed with HTTP {(int)response.StatusCode}.");
        }

        var body = await response.Content.ReadFromJsonAsync<MomoCreatePaymentResponse>(cancellationToken: ct);
        if (body is null)
        {
            return new CreatePaymentResult(
                Success: false,
                PaymentUrl: string.Empty,
                GatewayTransactionId: orderId,
                RedirectBehavior: RedirectBehavior,
                ErrorMessage: "MoMo create payment returned empty response.");
        }

        // Prefer direct QR/deeplink payload for QR-only UX, fallback to gateway web URL.
        string paymentUrl = RedirectBehavior switch
        {
            PaymentRedirectBehavior.QrCode => body.QrCodeUrl ?? body.Deeplink ?? body.PayUrl ?? string.Empty,
            _ => body.PayUrl ?? string.Empty
        };

        var success = body.ResultCode == 0 && !string.IsNullOrWhiteSpace(paymentUrl);

        logger.LogInformation("[MomoPayment] Create response: ResultCode={ResultCode}, Message={Message}, OrderId={OrderId}, PayUrl={PayUrl}",
            body.ResultCode, body.Message, body.OrderId, body.PayUrl);

        if (!success)
        {
            return new CreatePaymentResult(
                Success: false,
                PaymentUrl: string.Empty,
                GatewayTransactionId: orderId,
                RedirectBehavior: RedirectBehavior,
                ErrorMessage: body.Message ?? $"MoMo create payment failed ({body.ResultCode}).");
        }

        return new CreatePaymentResult(
            Success: true,
            PaymentUrl: paymentUrl,
            GatewayTransactionId: body.OrderId ?? orderId,
            RedirectBehavior: RedirectBehavior);
    }

    /// <summary>
    /// Verifies MoMo callback signature and result code.
    /// </summary>
    public Task<ConfirmPaymentResult> ConfirmPaymentAsync(
        ConfirmPaymentRequest request,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "MoMo is not configured."));
        }

        if (!request.GatewayResponseParams.TryGetValue("signature", out var receivedSignature)
            || string.IsNullOrWhiteSpace(receivedSignature))
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "Invalid MoMo signature."));
        }

        // MoMo callback/IPN signature (v2):
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message&orderId=$orderId
        // &orderInfo=$orderInfo&orderType=$orderType&partnerCode=$partnerCode&payType=$payType
        // &requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode&transId=$transId
        // accessKey comes from merchant credentials, not necessarily from callback body.
        var rawFields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["accessKey"] = _options.AccessKey,
            ["amount"] = request.GatewayResponseParams.TryGetValue("amount", out var amount) ? amount : string.Empty,
            ["extraData"] = request.GatewayResponseParams.TryGetValue("extraData", out var extraData) ? extraData : string.Empty,
            ["message"] = request.GatewayResponseParams.TryGetValue("message", out var ipnMessage) ? ipnMessage : string.Empty,
            ["orderId"] = request.GatewayResponseParams.TryGetValue("orderId", out var ipnOrderId) ? ipnOrderId : string.Empty,
            ["orderInfo"] = request.GatewayResponseParams.TryGetValue("orderInfo", out var orderInfo) ? orderInfo : string.Empty,
            ["orderType"] = request.GatewayResponseParams.TryGetValue("orderType", out var orderType) ? orderType : string.Empty,
            ["partnerCode"] = request.GatewayResponseParams.TryGetValue("partnerCode", out var partnerCode) ? partnerCode : string.Empty,
            ["payType"] = request.GatewayResponseParams.TryGetValue("payType", out var payType) ? payType : string.Empty,
            ["requestId"] = request.GatewayResponseParams.TryGetValue("requestId", out var requestId) ? requestId : string.Empty,
            ["responseTime"] = request.GatewayResponseParams.TryGetValue("responseTime", out var responseTime) ? responseTime : string.Empty,
            ["resultCode"] = request.GatewayResponseParams.TryGetValue("resultCode", out var resultCodeRaw) ? resultCodeRaw : string.Empty,
            ["transId"] = request.GatewayResponseParams.TryGetValue("transId", out var transId) ? transId : string.Empty
        };

        var orderedRaw = MomoSigning.BuildRawString(rawFields,
        [
            "accessKey",
            "amount",
            "extraData",
            "message",
            "orderId",
            "orderInfo",
            "orderType",
            "partnerCode",
            "payType",
            "requestId",
            "responseTime",
            "resultCode",
            "transId"
        ]);
        var expectedSignature = MomoSigning.ComputeHmacSha256(orderedRaw, _options.SecretKey);

        var signatureValid = string.Equals(receivedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);
        if (!signatureValid)
        {
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: request.GatewayTransactionId,
                ErrorMessage: "Invalid MoMo signature."));
        }

        request.GatewayResponseParams.TryGetValue("resultCode", out var resultCode);
        var isSuccess = string.Equals(resultCode, "0", StringComparison.Ordinal);
        var transactionOrderId = request.GatewayResponseParams.TryGetValue("orderId", out var callbackOrderId)
            ? callbackOrderId
            : request.GatewayTransactionId;

        if (!isSuccess)
        {
            request.GatewayResponseParams.TryGetValue("message", out var failureMessage);
            return Task.FromResult(new ConfirmPaymentResult(
                IsSuccess: false,
                BookingId: request.BookingId,
                GatewayTransactionId: transactionOrderId,
                ErrorMessage: failureMessage ?? $"MoMo payment failed ({resultCode ?? "unknown"})."));
        }

        return Task.FromResult(new ConfirmPaymentResult(
            IsSuccess: true,
            BookingId: request.BookingId,
            GatewayTransactionId: transactionOrderId));
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.PartnerCode)
               && !string.IsNullOrWhiteSpace(_options.AccessKey)
               && !string.IsNullOrWhiteSpace(_options.SecretKey)
               && !string.IsNullOrWhiteSpace(_options.CreateEndpoint);
    }

    private static string BuildOrderId(Guid paymentTransactionId)
    {
        return $"TXN-{paymentTransactionId:N}".ToUpperInvariant();
    }

    private string? BuildPublicUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return null;

        if (!Uri.TryCreate(_options.PublicBaseUrl, UriKind.Absolute, out var baseUri))
            return null;

        var normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;
        if (!normalizedPath.StartsWith('/'))
            normalizedPath = "/" + normalizedPath;

        return new Uri(baseUri, normalizedPath).ToString();
    }
}

internal sealed record MomoCreatePaymentRequest(
    [property: JsonPropertyName("partnerCode")] string PartnerCode,
    [property: JsonPropertyName("accessKey")] string AccessKey,
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("amount")] string Amount,
    [property: JsonPropertyName("orderId")] string OrderId,
    [property: JsonPropertyName("orderInfo")] string OrderInfo,
    [property: JsonPropertyName("redirectUrl")] string RedirectUrl,
    [property: JsonPropertyName("ipnUrl")] string IpnUrl,
    [property: JsonPropertyName("requestType")] string RequestType,
    [property: JsonPropertyName("extraData")] string ExtraData,
    [property: JsonPropertyName("lang")] string Lang,
    [property: JsonPropertyName("signature")] string Signature);

internal sealed record MomoCreatePaymentResponse(
    [property: JsonPropertyName("resultCode")] int ResultCode,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("payUrl")] string? PayUrl,
    [property: JsonPropertyName("deeplink")] string? Deeplink,
    [property: JsonPropertyName("qrCodeUrl")] string? QrCodeUrl,
    [property: JsonPropertyName("orderId")] string? OrderId);
