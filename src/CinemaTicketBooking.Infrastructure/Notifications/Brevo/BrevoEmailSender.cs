using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Infrastructure.Notifications.Brevo;

/// <summary>
/// Sends transactional emails via the Brevo (formerly Sendinblue) REST API v3.
/// Docs: https://developers.brevo.com/reference/sendtransacemail
/// </summary>
public sealed class BrevoEmailSender(
    IHttpClientFactory httpClientFactory,
    IOptions<BrevoOptions> options,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly BrevoOptions _options = options.Value;

    // =============================================
    // IEmailSender implementation
    // =============================================

    /// <inheritdoc />
    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? toName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        // 1. Build Brevo request payload
        var payload = new BrevoSendEmailRequest
        {
            Sender = new BrevoContact(_options.SenderName, _options.SenderEmail),
            To = [new BrevoContact(toName, to)],
            Subject = subject,
            HtmlContent = htmlBody
        };

        // 2. Add optional reply-to
        if (!string.IsNullOrWhiteSpace(_options.ReplyToEmail))
            payload.ReplyTo = new BrevoContact(null, _options.ReplyToEmail);

        // 3. Send
        await SendAsync(payload, cancellationToken);
    }

    // =============================================
    // Internal HTTP dispatch
    // =============================================

    /// <summary>
    /// Serialises the payload and POSTs it to the Brevo transactional email endpoint.
    /// Throws <see cref="HttpRequestException"/> on non-2xx responses.
    /// </summary>
    private async Task SendAsync(BrevoSendEmailRequest payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("Brevo ApiKey is not configured – email skipped.");
            return;
        }

        // 1. Create named HTTP client
        var client = httpClientFactory.CreateClient(nameof(BrevoEmailSender));

        // 2. Set auth header
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // 3. Serialise body
        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 4. POST to Brevo
        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/smtp/email";
        var response = await client.PostAsync(endpoint, content, cancellationToken);

        // 5. Handle error response
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "Brevo API returned {StatusCode}: {Body}",
                (int)response.StatusCode,
                body.Length > 500 ? body[..500] : body);

            response.EnsureSuccessStatusCode(); // throws HttpRequestException
        }
    }

    // =============================================
    // Request / response DTOs (internal)
    // =============================================

    private sealed class BrevoSendEmailRequest
    {
        public BrevoContact Sender { get; set; } = default!;
        public List<BrevoContact> To { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public BrevoContact? ReplyTo { get; set; }
    }

    private sealed record BrevoContact(string? Name, string Email);
}
