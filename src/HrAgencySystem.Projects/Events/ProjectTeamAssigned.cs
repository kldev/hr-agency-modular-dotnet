using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectTeamAssigned(
    Guid ProjectId,
    Guid OrganizationId,
    Guid TeamId,
    string TeamName,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
