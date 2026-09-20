namespace HrAgencySystem.Workers.Domain;

/// <summary>
/// A file about one posting. Same shape as a worker's document and deliberately a separate type:
/// they hang off different things and answering "which A1 covered this period" must not turn into a
/// search through everything the person ever handed in.
/// </summary>
public sealed record AssignmentDocument(
    Guid DocumentId,
    AssignmentDocumentCategory Category,
    Guid FileId,
    string FileName,
    string ContentType,
    long Size,
    DateOnly DocumentDate,
    DateOnly? ValidUntil,
    string? Note
);
