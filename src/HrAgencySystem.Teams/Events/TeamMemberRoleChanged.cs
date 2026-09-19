using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Teams.Domain;

namespace HrAgencySystem.Teams.Events;

public sealed record TeamMemberRoleChanged(
    Guid TeamId,
    Guid OrganizationId,
    TeamMemberSnapshot Member,
    TeamRole PreviousRole,
    UserSnapshot ChangedBy,
    DateTimeOffset OccurredAt
);
