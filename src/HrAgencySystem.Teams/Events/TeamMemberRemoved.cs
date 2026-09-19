using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Teams.Events;

public sealed record TeamMemberRemoved(
    Guid TeamId,
    Guid OrganizationId,
    TeamMemberSnapshot Member,
    UserSnapshot RemovedBy,
    DateTimeOffset OccurredAt
);
