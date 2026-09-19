using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Teams.Events;

public sealed record TeamCreated(
    Guid TeamId,
    Guid OrganizationId,
    string Name,
    IReadOnlyList<TeamMemberSnapshot> Members,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt
);
