using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Teams.Application.Port;

/// <summary>
/// Keeps "one person belongs to one team" true across aggregates. A team cannot see the rosters of
/// its siblings, so the rule lives where every team can reach it: a reservation row per member, with
/// a unique index that settles concurrent requests the friendly check cannot.
/// </summary>
public interface ITeamMembershipReservationRepository
{
    public const string AlreadyOnTeamMessage = "This person already belongs to another team.";

    /// <summary>
    /// Which of the given people are already on some team. Takes the whole list so founding a team
    /// costs one query rather than one per member.
    /// </summary>
    Task<IReadOnlyList<Guid>> FindAssignedAsync(
        OrganizationId organizationId,
        IReadOnlyList<Guid> userIds,
        CancellationToken ct
    );

    Task ReserveAsync(OrganizationId organizationId, Guid userId, Guid teamId);

    Task ReleaseAsync(OrganizationId organizationId, Guid userId, CancellationToken ct);
}
