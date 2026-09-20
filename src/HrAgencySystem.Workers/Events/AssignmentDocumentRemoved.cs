using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Workers.Events;

public sealed record AssignmentDocumentRemoved(
    Guid AssignmentId,
    Guid OrganizationId,
    Guid DocumentId,
    /// <summary>Carried so the endpoint can delete the bytes after the record is gone.</summary>
    Guid FileId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
