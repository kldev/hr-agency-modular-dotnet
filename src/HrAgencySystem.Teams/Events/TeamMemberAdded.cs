using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Teams.Events;

public sealed record TeamMemberAdded(
    Guid TeamId,
    Guid OrganizationId,
    TeamMemberSnapshot Member,
    UserSnapshot AddedBy,
    DateTimeOffset OccurredAt
);
