namespace CinemaTicketBooking.Domain;

/// <summary>
/// Determines how the client handles payment presentation.
/// </summary>
public enum PaymentRedirectBehavior
{
    /// <summary>
    /// Navigate to external payment gateway URL.
    /// </summary>
    Redirect,

    /// <summary>
    /// Display QR code inline in frontend.
    /// </summary>
    QrCode
}
