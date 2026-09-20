using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

/// <summary>
/// Removal is a fact worth keeping. A compliance file that was there and is not any more is exactly
/// the sort of thing somebody will later need to account for.
/// </summary>
public sealed record ProjectDocumentRemoved(
    Guid ProjectId,
    Guid OrganizationId,
    Guid DocumentId,
    Guid FileId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
