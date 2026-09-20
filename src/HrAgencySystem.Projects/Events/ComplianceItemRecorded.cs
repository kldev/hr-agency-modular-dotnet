using HrAgencySystem.Projects.Domain.Compliance;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ComplianceItemRecorded(
    Guid ProjectId,
    Guid OrganizationId,
    ComplianceItem Item,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
