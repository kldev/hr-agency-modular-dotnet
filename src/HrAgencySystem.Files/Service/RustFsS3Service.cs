using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HrAgencySystem.Files.Config;
using HrAgencySystem.Files.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Files.Service;

public class RustFsS3Service : IFileStorage
{
    private readonly ILogger<RustFsS3Service> _logger;
    private readonly AmazonS3Client _amazonS3;

    public RustFsS3Service(IOptions<S3Config> configuration, ILogger<RustFsS3Service> logger)
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
            RegionEndpoint = Amazon.RegionEndpoint.USEast1,
            ServiceURL = s3Config.Endpoint,
            ForcePathStyle = true
        };

        return (credentials, config);
    }

    
    private  async Task<FileResponse> GetObjectStreamAsync(string key, CancellationToken token, string useBucketName = "")
    {
        var request = new GetObjectRequest
        {
            Key = key,
            BucketName = string.IsNullOrEmpty(useBucketName) ? BucketNames.FeedJobs : useBucketName
        };
        
        try
        {
            var response = await _amazonS3.GetObjectAsync(request, token);
            
            _logger.LogDebug($"Response: {response.HttpStatusCode}");
            if (response is { HttpStatusCode:  HttpStatusCode.OK}) 
                return FileResponse.SuccessAction(response.ResponseStream, response.Headers.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
        
        return FileResponse.FailureAction("file_not_found");
    }

    public async Task EnsureBucketExistsAsync(
        CancellationToken token,
        string bucketName = "")
    {
        try
        {
            await _amazonS3.HeadBucketAsync(
                new HeadBucketRequest
                {
                    BucketName = bucketName
                },
                token);
        }
        catch (AmazonS3Exception e)
            when (e.StatusCode == HttpStatusCode.NotFound)
        {
            await CreateBucketAsync(bucketName, token);
        }
    }

    private async Task CreateBucketAsync(string useBucketName, CancellationToken ct)
    {
        var request = new PutBucketRequest
        {
            BucketName = useBucketName
        };
        await _amazonS3.PutBucketAsync(request, ct);
    }

    public async Task StoreAsync(FileInput input, string key, string bucketName, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken, bucketName);
        var request = new PutObjectRequest
        {
            Key = key,
            BucketName = bucketName, 
            ContentType = input.ContentType, InputStream = input.Content, 
            
        };

        await _amazonS3.PutObjectAsync(request, cancellationToken);

    }

    public async Task<FileResponse> GetAsync(string key, string bucketName, CancellationToken cancellationToken)
    {
        return await GetObjectStreamAsync(key, cancellationToken, bucketName);
    }
}