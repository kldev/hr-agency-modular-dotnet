using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectPositionRestored(
    Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
