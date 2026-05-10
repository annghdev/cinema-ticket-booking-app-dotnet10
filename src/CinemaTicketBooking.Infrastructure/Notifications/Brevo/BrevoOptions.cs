namespace CinemaTicketBooking.Infrastructure.Notifications;

/// <summary>
/// Configuration for the Brevo (formerly Sendinblue) transactional email API.
/// Bound from the "BrevoSettings" section in appsettings.json.
/// </summary>
public sealed class BrevoOptions
{
    public const string SectionName = "BrevoSettings";

    /// <summary>Brevo API key (v3).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Verified sender email address.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Display name shown in the From field.</summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Optional reply-to address. Defaults to SenderEmail when empty.
    /// </summary>
    public string ReplyToEmail { get; set; } = string.Empty;

    /// <summary>
    /// Brevo transactional API base URL.
    /// Defaults to the official v3 endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.brevo.com/v3";
}
