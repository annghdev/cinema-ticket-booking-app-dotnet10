using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CinemaTicketBooking.Infrastructure.Payments.Vnpay;

/// <summary>
/// Provides VNPay canonical query building and HMAC-SHA512 signing helpers.
/// </summary>
internal static class VnpayHashing
{
    /// <summary>
    /// Builds a canonical query string from parameters sorted by key.
    /// </summary>
    public static string BuildCanonicalQuery(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        return string.Join("&", parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
    }

    /// <summary>
    /// Computes the VNPay HMAC-SHA512 signature from canonical data.
    /// </summary>
    public static string ComputeSignature(string canonicalData, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(canonicalData);
        var hash = HMACSHA512.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hash);
    }
}
