namespace CinemaTicketBooking.Application.Abstractions;

public interface IFileStorageService
{
    Task<UploadedFile> UploadAsync(
        UploadFileRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string objectKey);

    Task<string> GetPresignedUrlAsync(
        string objectKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}


public class UploadFileRequest
{
    public required Stream Stream { get; init; }

    public required string ObjectKey { get; init; }

    public required string ContentType { get; init; }

    public string? Group { get; init; }

    public long? Size { get; init; }

    public bool IsPublic { get; init; } = true;
}

public class UploadedFile
{
    public required string ObjectKey { get; init; }

    public required string Url { get; init; }

    public required string ContentType { get; init; }

    public long? Size { get; init; }
}