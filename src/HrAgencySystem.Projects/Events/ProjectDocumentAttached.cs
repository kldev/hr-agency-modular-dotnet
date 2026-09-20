using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectDocumentAttached(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectDocument Document,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
