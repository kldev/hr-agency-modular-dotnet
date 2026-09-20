using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectStatusChanged(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectStatus PreviousStatus,
    ProjectStatus Status,
    string Reason,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt
);
