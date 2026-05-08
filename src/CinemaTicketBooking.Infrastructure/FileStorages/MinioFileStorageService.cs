using CinemaTicketBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace CinemaTicketBooking.Infrastructure.FileStorages;

/// <summary>
/// MinIO implementation of <see cref="IFileStorageService"/>.
/// Supports upload, delete, public URL generation, and pre-signed URLs.
/// </summary>
public class MinioFileStorageService(
    IMinioClient minioClient,
    IOptions<MinioOptions> options,
    ILogger<MinioFileStorageService> logger) : IFileStorageService
{
    private readonly MinioOptions _options = options.Value;

    /// <summary>
    /// Uploads a file stream to MinIO and returns the stored object metadata.
    /// </summary>
    public async Task<UploadedFile> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Ensure bucket exists
        await EnsureBucketExistsAsync(cancellationToken);

        // 2. Build the full object key with optional group prefix
        var objectKey = string.IsNullOrWhiteSpace(request.Group)
            ? request.ObjectKey
            : $"{request.Group.TrimEnd('/')}/{request.ObjectKey}";

        // 3. Upload object to MinIO
        var putArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(request.Stream)
            .WithObjectSize(request.Size ?? request.Stream.Length)
            .WithContentType(request.ContentType);

        await minioClient.PutObjectAsync(putArgs, cancellationToken);

        logger.LogDebug("Uploaded object '{ObjectKey}' to bucket '{Bucket}'", objectKey, _options.BucketName);

        return new UploadedFile
        {
            ObjectKey = objectKey,
            Url = GetPublicUrl(objectKey),
            ContentType = request.ContentType,
            Size = request.Size ?? request.Stream.Length
        };
    }

    /// <summary>
    /// Deletes an object from MinIO by its key.
    /// </summary>
    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey);

        await minioClient.RemoveObjectAsync(removeArgs, cancellationToken);

        logger.LogDebug("Deleted object '{ObjectKey}' from bucket '{Bucket}'", objectKey, _options.BucketName);
    }

    /// <summary>
    /// Returns the public URL for a stored object.
    /// Falls back to returning the input directly if it is already a full URL
    /// (supports legacy entities that stored external provider URLs).
    /// </summary>
    public string GetPublicUrl(string objectKey)
    {
        // Fallback: if objectKey is already a complete URL, return it as-is
        if (Uri.IsWellFormedUriString(objectKey, UriKind.Absolute))
        {
            return objectKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return $"{_options.PublicBaseUrl.TrimEnd('/')}/{objectKey}";
        }

        var scheme = _options.UseSsl ? "https" : "http";
        return $"{scheme}://{_options.Endpoint}/{_options.BucketName}/{objectKey}";
    }

    /// <summary>
    /// Generates a time-limited pre-signed URL for private object access.
    /// </summary>
    public async Task<string> GetPresignedUrlAsync(
        string objectKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry((int)expiration.TotalSeconds);

        var url = await minioClient.PresignedGetObjectAsync(presignedArgs);
        return url;
    }

    // =============================================
    // Internal helpers
    // =============================================

    private static bool _bucketInitialized;

    /// <summary>
    /// Ensures the configured bucket exists with public read policy.
    /// Uses a static flag to run only once per application lifetime.
    /// </summary>
    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (_bucketInitialized) return;

        var existsArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
        var exists = await minioClient.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(_options.BucketName);
            await minioClient.MakeBucketAsync(makeArgs, cancellationToken);
            logger.LogInformation("Created MinIO bucket '{Bucket}'", _options.BucketName);
        }

        // Always apply public read policy (idempotent) to handle
        // buckets created before the policy was introduced
        await SetPublicReadPolicyAsync(cancellationToken);

        _bucketInitialized = true;
    }

    /// <summary>
    /// Sets a public read-only bucket policy (anonymous s3:GetObject) on the configured bucket.
    /// </summary>
    private async Task SetPublicReadPolicyAsync(CancellationToken cancellationToken)
    {
        var policyJson = $$"""
        {
            "Version": "2012-10-17",
            "Statement": [
                {
                    "Effect": "Allow",
                    "Principal": { "AWS": ["*"] },
                    "Action": ["s3:GetObject"],
                    "Resource": ["arn:aws:s3:::{{_options.BucketName}}/*"]
                }
            ]
        }
        """;

        var policyArgs = new SetPolicyArgs()
            .WithBucket(_options.BucketName)
            .WithPolicy(policyJson);

        await minioClient.SetPolicyAsync(policyArgs, cancellationToken);
    }
}
