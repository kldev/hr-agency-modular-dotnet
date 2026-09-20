using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HrAgencySystem.Files.Config;
using HrAgencySystem.Files.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Files.Service;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly ILogger<S3ObjectStorage> _logger;
    private readonly AmazonS3Client _amazonS3;

    public S3ObjectStorage(IOptions<S3Config> configuration, ILogger<S3ObjectStorage> logger)
    {
        _logger = logger;
        var (credentials, config) = GetConfig(configuration.Value);

        _amazonS3 = new AmazonS3Client(credentials, config);
    }

    private static (BasicAWSCredentials, AmazonS3Config) GetConfig(S3Config s3Config)
    {
        var credentials = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Config.Region),
            ServiceURL = s3Config.Endpoint,
            ForcePathStyle = s3Config.ForcePathStyle,
        };

        return (credentials, config);
    }

    public async Task StoreAsync(
        FileInput input,
        string key,
        string bucketName,
        CancellationToken cancellationToken
    )
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var request = new PutObjectRequest
        {
            Key = key,
            BucketName = bucketName,
            ContentType = input.ContentType,
            InputStream = input.Content,
        };

        await _amazonS3.PutObjectAsync(request, cancellationToken);
    }

    public async Task<FileResponse> GetAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken
    )
    {
        var request = new GetObjectRequest { Key = key, BucketName = bucketName };

        try
        {
            var response = await _amazonS3.GetObjectAsync(request, cancellationToken);

            return FileResponse.SuccessAction(
                response.ResponseStream,
                response.Headers.ContentType
            );
        }
        catch (AmazonS3Exception ex) when (IsMissing(ex))
        {
            // A missing object is an answer, not a failure. Everything else - bad credentials,
            // an unreachable endpoint, a broken bucket policy - has to reach the caller, because
            // reporting it as "file not found" turns every outage into a wild goose chase.
            _logger.LogDebug("Object {Key} not found in bucket {Bucket}.", key, bucketName);
            return FileResponse.FailureAction("file_not_found");
        }
    }

    public async Task DeleteAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken
    )
    {
        var request = new DeleteObjectRequest { Key = key, BucketName = bucketName };

        try
        {
            await _amazonS3.DeleteObjectAsync(request, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (IsMissing(ex))
        {
            // Deleting what is already gone is the outcome the caller asked for.
            _logger.LogDebug("Object {Key} already absent from bucket {Bucket}.", key, bucketName);
        }
    }

    public async Task<bool> ExistsAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken
    )
    {
        return await GetMetadataAsync(key, bucketName, cancellationToken) is not null;
    }

    public async Task<ObjectMetadata?> GetMetadataAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken
    )
    {
        var request = new GetObjectMetadataRequest { Key = key, BucketName = bucketName };

        try
        {
            var response = await _amazonS3.GetObjectMetadataAsync(request, cancellationToken);

            return new ObjectMetadata(
                response.Headers.ContentType ?? "application/octet-stream",
                response.Headers.ContentLength,
                response.LastModified ?? DateTimeOffset.MinValue,
                response.ETag ?? string.Empty
            );
        }
        catch (AmazonS3Exception ex) when (IsMissing(ex))
        {
            return null;
        }
    }

    public Uri CreatePresignedGetUrl(string key, string bucketName, TimeSpan expiresIn)
    {
        var request = new GetPreSignedUrlRequest
        {
            Key = key,
            BucketName = bucketName,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiresIn),
        };

        return new Uri(_amazonS3.GetPreSignedURL(request));
    }

    private static bool IsMissing(AmazonS3Exception exception) =>
        exception.StatusCode is HttpStatusCode.NotFound;

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct)
    {
        try
        {
            await _amazonS3.HeadBucketAsync(new HeadBucketRequest { BucketName = bucketName }, ct);
        }
        catch (AmazonS3Exception ex) when (IsMissing(ex))
        {
            await _amazonS3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, ct);
        }
    }
}
