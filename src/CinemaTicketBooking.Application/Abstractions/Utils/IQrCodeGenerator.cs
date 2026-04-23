namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Produces a short string persisted on the booking and returned to clients as the check-in token.
/// The value is the same text that should be encoded in a QR (e.g. booking id), not an image.
/// </summary>
public interface IQrCodeGenerator
{
    string GenerateCode(string input);
    Task<string> GenerateCodeAsync(string input);
}
