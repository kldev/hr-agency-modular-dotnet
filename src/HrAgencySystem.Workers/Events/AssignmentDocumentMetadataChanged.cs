using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

public sealed record AssignmentDocumentMetadataChanged(
    Guid AssignmentId,
    Guid OrganizationId,
    AssignmentDocument Document,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
