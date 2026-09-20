namespace HrAgencySystem.Teams.Contracts.IntegrationEvents;

/// <summary>
/// Where one person stands after a change to a team. A single fact, not a delta: a member belongs to
/// exactly one team or to none, so <paramref name="TeamId"/> being null is the value "no team", never
/// a discriminator for some other payload.
///
/// Renaming a team emits one of these per member, because everyone holding a copy of the name has to
/// hear about it.
/// </summary>
public sealed record TeamMembershipChanged(
    Guid UserId,
    Guid OrganizationId,
    Guid? TeamId,
    string? TeamName,
    TeamRole? Role,
    DateTimeOffset OccurredAt
)
{
    public static TeamMembershipChanged OnTeam(
        Guid userId,
        Guid organizationId,
        Guid teamId,
        string teamName,
        TeamRole role,
        DateTimeOffset occurredAt
    )
    {
        return new TeamMembershipChanged(
            userId,
            organizationId,
            teamId,
            teamName,
            role,
            occurredAt
        );
    }

    public static TeamMembershipChanged NoTeam(
        Guid userId,
        Guid organizationId,
        DateTimeOffset occurredAt
    )
    {
        return new TeamMembershipChanged(userId, organizationId, null, null, null, occurredAt);
    }
}
