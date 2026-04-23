using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Infrastructure.QrCodes;

/// <summary>
/// Produces the persisted check-in token for a booking. The value is the plain payload to encode in
/// a QR (typically the booking Guid as a string), so mobile scanners and gate systems resolve the same
/// id as the API. Image rendering is a client concern, not database storage.
/// </summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    /// <inheritdoc />
    public string GenerateCode(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input.Trim();
    }

    /// <inheritdoc />
    public Task<string> GenerateCodeAsync(string input) =>
        Task.FromResult(GenerateCode(input));
}
