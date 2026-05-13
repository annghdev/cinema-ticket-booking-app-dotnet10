namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Uploads an image to file storage under a given group folder.
/// Does NOT require an existing entity — suitable for both create and update flows.
/// </summary>
public class UploadImageCommand : ICommand
{
    /// <summary>
    /// Logical group/folder prefix (e.g. "movies", "cinemas", "concessions").
    /// </summary>
    public required string Group { get; set; }

    public required Stream FileStream { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles image upload by storing the file and returning the public URL.
/// </summary>
public class UploadImageCommandHandler(IFileStorageService fileStorage)
{
    /// <summary>
    /// Stores the image in file storage and returns the public URL.
    /// </summary>
    public async Task<string> Handle(UploadImageCommand command, CancellationToken ct)
    {
        // 1. Build unique object key
        var extension = Path.GetExtension(command.FileName);
        var objectKey = $"{command.Group}/{Guid.CreateVersion7()}{extension}";

        // 2. Upload file
        var uploaded = await fileStorage.UploadAsync(new UploadFileRequest
        {
            Stream = command.FileStream,
            ObjectKey = objectKey,
            ContentType = command.ContentType,
            Size = command.FileSize,
            IsPublic = true
        }, ct);

        return uploaded.Url;
    }
}
