namespace CinemaTicketBooking.Domain;

/// <summary>
/// Supported payment gateway methods.
/// </summary>
public enum PaymentMethod
{
    None,       // Development/fake gateway
    VnPay,
    Momo,
    VietQR
}
