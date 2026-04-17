using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CinemaTicketBooking.Infrastructure.Notifications;

/// <summary>
/// Development-friendly email sender that logs the message body.
/// </summary>
public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email to {To} subject {Subject}: {Body}",
            to,
            subject,
            htmlBody.Length > 500 ? htmlBody[..500] + "…" : htmlBody);
        return Task.CompletedTask;
    }
}
