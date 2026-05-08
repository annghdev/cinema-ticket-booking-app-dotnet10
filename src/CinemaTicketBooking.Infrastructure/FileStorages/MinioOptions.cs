namespace CinemaTicketBooking.Infrastructure.FileStorages;

/// <summary>
/// Configuration options for MinIO object storage connection.
/// </summary>
public sealed class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "cinema-ticket-booking";
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Public base URL for direct access (e.g., CDN or reverse-proxy URL).
    /// When set, GetPublicUrl returns "{PublicBaseUrl}/{objectKey}".
    /// When empty, falls back to "http(s)://{Endpoint}/{BucketName}/{objectKey}".
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}
