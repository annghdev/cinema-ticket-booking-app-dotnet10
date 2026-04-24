using System.Security.Cryptography;
using System.Text;

namespace CinemaTicketBooking.Infrastructure.Payments.Momo;

/// <summary>
/// Provides helper methods to build and verify MoMo HMAC signatures.
/// </summary>
internal static class MomoSigning
{
    /// <summary>
    /// Builds signature raw string using provided key order.
    /// </summary>
    public static string BuildRawString(
        IDictionary<string, string> fields,
        IReadOnlyList<string> keyOrder)
    {
        var segments = new List<string>(keyOrder.Count);
        foreach (var key in keyOrder)
        {
            if (fields.TryGetValue(key, out var value))
            {
                segments.Add($"{key}={value}");
            }
        }

        return string.Join("&", segments);
    }

    /// <summary>
    /// Builds signature raw string by sorting keys ordinally.
    /// </summary>
    public static string BuildSortedRawString(IDictionary<string, string> fields)
    {
        return string.Join("&", fields
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}"));
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature.
    /// </summary>
    public static string ComputeHmacSha256(string rawData, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);
        var hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
