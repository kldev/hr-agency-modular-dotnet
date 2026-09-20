namespace HrAgencySystem.FileService.Domain;

/// <summary>
/// The record of one uploaded file. A plain Marten document, not an event stream: a file has no life
/// beyond "it is there" and "it is not". The story of the document that points at it belongs to the
/// module that owns that document.
/// </summary>
public sealed class StoredFile
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string OwnerKind { get; init; } = "";
    public Guid OwnerId { get; init; }
    public string StorageKey { get; init; } = "";
    public string Bucket { get; init; } = "";
    public string FileName { get; init; } = "";
    public string ContentType { get; init; } = "";
    public long Size { get; init; }
    public string Sha256 { get; init; } = "";
    public Guid UploadedBy { get; init; }
    public DateTimeOffset UploadedAt { get; init; }

    /// <summary>
    /// Soft delete. The row outlives the object so that a later question - who uploaded this, when,
    /// and who removed it - still has an answer.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}
