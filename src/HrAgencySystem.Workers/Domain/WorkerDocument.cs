namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// A file about a person, described by what it is rather than by where it is stored.
/// <para>
/// <paramref name="FileId"/> is the whole of what the domain knows about storage. No bucket, no key,
/// no provider - resolving it into bytes is the file service's job.
/// </para>
/// </summary>
public sealed record WorkerDocument(
    Guid DocumentId,
    WorkerDocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note
);
