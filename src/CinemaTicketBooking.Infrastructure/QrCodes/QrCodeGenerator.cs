using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;
using QRCoder;

namespace CinemaTicketBooking.Infrastructure.QrCodes;

/// <summary>
/// Renders booking check-in QR codes as PNG data URLs so clients can show them immediately after payment,
/// while scanners read the plain booking id payload for automatic gate lookup.
/// </summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    private const string DataUrlPrefix = "data:image/png;base64,";

    /// <inheritdoc />
    public string GenerateCode(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        // 1. Payload is the booking id string — gate apps can parse Guid after scan without extra decoding.
        var payload = input.Trim();

        // 2. Shrink module size until the data URL fits QrCode column (MaxLengthConsts).
        var maxLength = MaxLengthConsts.QrCode;
        foreach (var pixelsPerModule in new[] { 6, 5, 4, 3, 2 })
        {
            var png = RenderPng(payload, pixelsPerModule);
            var dataUrl = $"{DataUrlPrefix}{Convert.ToBase64String(png)}";
            if (dataUrl.Length <= maxLength)
                return dataUrl;
        }

        throw new InvalidOperationException(
            "Generated QR image exceeds MaxLengthConsts.QrCode; increase the limit or reduce ECC/payload.");
    }

    /// <inheritdoc />
    public Task<string> GenerateCodeAsync(string input) =>
        Task.FromResult(GenerateCode(input));

    private static byte[] RenderPng(string payload, int pixelsPerModule)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}
