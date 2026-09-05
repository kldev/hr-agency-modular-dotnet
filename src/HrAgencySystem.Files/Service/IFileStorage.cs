using HrAgencySystem.Files.Model;

namespace HrAgencySystem.Files.Service;

public interface IFileStorage
{
    Task StoreAsync(
        FileInput input,
        string key,
        string bucketName,
        CancellationToken cancellationToken);
    
    Task<FileResponse> GetAsync(
        string key,
        string bucketName,
        CancellationToken cancellationToken);

}