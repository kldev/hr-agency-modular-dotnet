using System.Security.Cryptography;
using HrAgencySystem.Files.Model;
using HrAgencySystem.Files.Service;
using HrAgencySystem.FileService.Contracts;
using HrAgencySystem.FileService.Domain;
using Marten;

namespace HrAgencySystem.FileService.Application;

public sealed class FileStore(
    IDocumentSession session,
    IObjectStorage storage,
    IUploadInspector inspector,
    TimeProvider clock,
    ILogger<FileStore> logger
) : IFileStore
{
    public async Task<StoreResult> StoreAsync(
        Guid organizationId,
        FileOwnerRef owner,
        Guid uploadedBy,
        Stream content,
        long size,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        var name = FileName.Sanitize(fileName);

        var rejection = inspector.Inspect(contentType, name, size);
        if (rejection is not null)
            return StoreResult.Rejected(rejection);

        var fileId = Guid.NewGuid();
        var key = StorageKey.Create(
            organizationId,
            owner.Kind,
            owner.Id,
            fileId,
            inspector.ExtensionFor(contentType)
        );

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        var sha256 = Convert.ToHexStringLower(
            SHA256.HashData(buffer.GetBuffer().AsSpan(0, (int)buffer.Length))
        );

        var stored = new StoredFile
        {
            Id = fileId,
            OrganizationId = organizationId,
            OwnerKind = StorageKey.NormalizeOwnerKind(owner.Kind),
            OwnerId = owner.Id,
            StorageKey = key,
            Bucket = BucketNames.Documents,
            FileName = name,
            ContentType = contentType,
            Size = buffer.Length,
            Sha256 = sha256,
            UploadedBy = uploadedBy,
            UploadedAt = clock.GetUtcNow(),
        };

        // The object goes up before the row is committed. The other order would leave a row pointing
        // at nothing whenever the upload failed - a file that looks present and never opens, which is
        // worse than the orphan object this order can leave behind.
        session.Insert(stored);
        await storage.StoreAsync(
            new FileInput(buffer, name, contentType),
            key,
            BucketNames.Documents,
            ct
        );
        await session.SaveChangesAsync(ct);

        return StoreResult.Stored(Describe(stored));
    }

    public async Task<FileDescriptor?> GetAsync(
        Guid organizationId,
        Guid fileId,
        CancellationToken ct
    )
    {
        var stored = await Find(organizationId, fileId, ct);
        return stored is null ? null : Describe(stored);
    }

    public async Task<FileContent?> OpenReadAsync(
        Guid organizationId,
        Guid fileId,
        CancellationToken ct
    )
    {
        var stored = await Find(organizationId, fileId, ct);
        if (stored is null)
            return null;

        var response = await storage.GetAsync(stored.StorageKey, stored.Bucket, ct);
        if (response.FileNotFound)
        {
            logger.LogWarning(
                "File {FileId} is recorded but its object {Key} is missing from bucket {Bucket}.",
                stored.Id,
                stored.StorageKey,
                stored.Bucket
            );
            return null;
        }

        return new FileContent(
            response.OutputStream!,
            stored.FileName,
            stored.ContentType,
            stored.Size
        );
    }

    public async Task<bool> DeleteAsync(
        Guid organizationId,
        Guid fileId,
        Guid deletedBy,
        CancellationToken ct
    )
    {
        var stored = await Find(organizationId, fileId, ct);
        if (stored is null)
            return false;

        stored.DeletedAt = clock.GetUtcNow();
        stored.DeletedBy = deletedBy;

        // The record is marked first. If the object removal then fails we are left with bytes nobody
        // can reach, which is untidy; the reverse order leaves a live record over deleted bytes,
        // which is a lie.
        session.Update(stored);
        await session.SaveChangesAsync(ct);

        try
        {
            await storage.DeleteAsync(stored.StorageKey, stored.Bucket, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "File {FileId} was marked deleted but its object {Key} could not be removed.",
                stored.Id,
                stored.StorageKey
            );
        }

        return true;
    }

    private Task<StoredFile?> Find(Guid organizationId, Guid fileId, CancellationToken ct) =>
        session
            .Query<StoredFile>()
            .Where(f => f.Id == fileId && f.OrganizationId == organizationId && f.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

    private static FileDescriptor Describe(StoredFile stored) =>
        new(
            stored.Id,
            stored.FileName,
            stored.ContentType,
            stored.Size,
            stored.Sha256,
            stored.UploadedAt
        );
}
