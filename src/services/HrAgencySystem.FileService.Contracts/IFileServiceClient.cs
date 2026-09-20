namespace HrAgencySystem.FileService.Contracts;

/// <summary>
/// The whole of what the monolith may ask of the file service.
/// <para>
/// Every operation carries the organization: the caller always knows its tenant, and "does this id
/// exist anywhere" is a question nobody here needs answered. A file belonging to another
/// organization is reported as absent, never as forbidden - saying "forbidden" would confirm that
/// it exists.
/// </para>
/// </summary>
public interface IFileServiceClient
{
    Task<FileDescriptor> UploadAsync(
        Guid organizationId,
        FileOwnerRef owner,
        Guid uploadedBy,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct
    );

    Task<FileContent?> DownloadAsync(Guid organizationId, Guid fileId, CancellationToken ct);

    Task<FileDescriptor?> GetAsync(Guid organizationId, Guid fileId, CancellationToken ct);

    Task DeleteAsync(Guid organizationId, Guid fileId, Guid deletedBy, CancellationToken ct);
}
