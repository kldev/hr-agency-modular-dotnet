using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectDocumentMetadataChanged(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectDocument Document,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
