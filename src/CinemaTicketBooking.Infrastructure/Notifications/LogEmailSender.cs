using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CinemaTicketBooking.Infrastructure.Notifications;

/// <summary>
/// Development-friendly email sender that logs messages instead of sending real email.
/// Swap for <see cref="Brevo.BrevoEmailSender"/> in production via DI registration.
/// </summary>
public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task SendEmailAsync(string to, string subject, string htmlBody, string? toName = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[DEV] Email → {To} ({Name}) | Subject: {Subject} | Body (first 500): {Body}",
            to,
            toName ?? "no name",
            subject,
            htmlBody.Length > 500 ? htmlBody[..500] + "…" : htmlBody);

        return Task.CompletedTask;
    }
}
