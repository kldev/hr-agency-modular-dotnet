using HrAgencySystem.Files.Model;

namespace HrAgencySystem.Files.Service;

/// <summary>
/// The object store, and nothing above it: keys and buckets are the caller's vocabulary, ownership
/// and tenancy are not this layer's business. Whoever calls it is responsible for building a key
/// that cannot collide with another organization's.
/// </summary>
public interface IObjectStorage
{
    Task StoreAsync(
        FileInput input,
        string key,
        string bucketName,
        CancellationToken cancellationToken
    );

    Task<FileResponse> GetAsync(string key, string bucketName, CancellationToken cancellationToken);

    Task DeleteAsync(string key, string bucketName, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string key, string bucketName, CancellationToken cancellationToken);

    Task<ObjectMetadata?> GetMetadataAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken
    );
}
