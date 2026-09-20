using HrAgencySystem.FileService.Contracts;

namespace HrAgencySystem.FileService.Application;

public interface IFileStore
{
    Task<StoreResult> StoreAsync(
        Guid organizationId,
        FileOwnerRef owner,
        Guid uploadedBy,
        Stream content,
        long size,
        string fileName,
        string contentType,
        CancellationToken ct
    );

    Task<FileDescriptor?> GetAsync(Guid organizationId, Guid fileId, CancellationToken ct);

    Task<FileContent?> OpenReadAsync(Guid organizationId, Guid fileId, CancellationToken ct);

    Task<bool> DeleteAsync(Guid organizationId, Guid fileId, Guid deletedBy, CancellationToken ct);
}

/// <summary>
/// Either the stored file or the reason it was refused. A refusal is an ordinary answer here, not an
/// exception: "this type is not accepted" is something the caller has to show a person.
/// </summary>
public sealed record StoreResult(FileDescriptor? File, string? Rejection)
{
    public static StoreResult Stored(FileDescriptor file) => new(file, null);

    public static StoreResult Rejected(string reason) => new(null, reason);
}
