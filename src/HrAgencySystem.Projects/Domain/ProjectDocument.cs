namespace HrAgencySystem.Projects.Domain;

/// <summary>
/// A file attached to the project, described by what it is rather than by where it is stored.
/// <para>
/// <paramref name="FileId"/> is the whole of what the domain knows about storage. No bucket, no key,
/// no provider - resolving it into bytes is the file service's job.
/// </para>
/// </summary>
public sealed record ProjectDocument(
    Guid DocumentId,
    DocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note
);
