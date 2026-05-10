namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Sends transactional email (password reset, notifications).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends a raw HTML email message.
    /// </summary>
    /// <param name="to">Recipient address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="toName">Optional recipient name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(string to, string subject, string htmlBody, string? toName = null, CancellationToken cancellationToken = default);
}
