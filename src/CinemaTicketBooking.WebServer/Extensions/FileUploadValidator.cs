using Microsoft.AspNetCore.Http;

namespace CinemaTicketBooking.WebServer.Extensions;

/// <summary>
/// Provides security-focused file validation for image uploads.
/// Validates extension whitelist, content-type, magic bytes, and size limits.
/// </summary>
public static class FileUploadValidator
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/avif"
    };

    /// <summary>
    /// Magic-byte signatures for common image formats (first N bytes).
    /// </summary>
    private static readonly byte[][] MagicBytes =
    [
        [0xFF, 0xD8, 0xFF],                     // JPEG
        [0x89, 0x50, 0x4E, 0x47],               // PNG
        [0x47, 0x49, 0x46],                      // GIF
        [0x52, 0x49, 0x46, 0x46],               // WebP (RIFF header)
    ];

    /// <summary>
    /// Default max file size: 5 MB.
    /// </summary>
    public const long DefaultMaxFileSize = 5 * 1024 * 1024;

    /// <summary>
    /// Validates an uploaded image file for extension, content type, magic bytes, and size.
    /// Returns null when valid, or an error message string when invalid.
    /// </summary>
    public static string? ValidateImageFile(IFormFile file, long maxSize = DefaultMaxFileSize)
    {
        // 1. Check file presence
        if (file is null || file.Length == 0)
        {
            return "No file was uploaded.";
        }

        // 2. Check file size
        if (file.Length > maxSize)
        {
            return $"File exceeds maximum allowed size of {maxSize / (1024 * 1024)} MB.";
        }

        // 3. Check extension whitelist
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}.";
        }

        // 4. Check content type
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return $"Content type '{file.ContentType}' is not allowed.";
        }

        // 5. Validate magic bytes (prevent disguised malicious files)
        using var reader = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = reader.Read(header, 0, header.Length);
        reader.Position = 0;

        if (bytesRead < 3)
        {
            return "File is too small to be a valid image.";
        }

        var hasMagicMatch = false;
        foreach (var magic in MagicBytes)
        {
            if (bytesRead >= magic.Length && header.AsSpan(0, magic.Length).SequenceEqual(magic))
            {
                hasMagicMatch = true;
                break;
            }
        }

        // AVIF uses the ISOBMFF container — header starts at byte 4 with 'ftyp'
        if (!hasMagicMatch && bytesRead >= 8)
        {
            var ftypSpan = header.AsSpan(4, 4);
            if (ftypSpan.SequenceEqual("ftyp"u8))
            {
                hasMagicMatch = true;
            }
        }

        if (!hasMagicMatch)
        {
            return "File content does not match a valid image format.";
        }

        return null;
    }
}
