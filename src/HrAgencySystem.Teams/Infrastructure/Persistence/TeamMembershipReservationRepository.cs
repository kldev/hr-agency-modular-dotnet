using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Port;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Persistence;

public sealed class TeamMembershipReservationRepository(IDocumentSession session)
    : ITeamMembershipReservationRepository
{
    public async Task<IReadOnlyList<Guid>> FindAssignedAsync(
        OrganizationId organizationId,
        IReadOnlyList<Guid> userIds,
        CancellationToken ct
    )
    {
        if (userIds.Count == 0)
            return [];

        return await session
            .Query<TeamMembershipReservation>()
            .Where(r => r.OrganizationId == organizationId.Value && userIds.Contains(r.UserId))
            .Select(r => r.UserId)
            .ToListAsync(ct);
    }

    public Task ReserveAsync(OrganizationId organizationId, Guid userId, Guid teamId)
    {
        session.Insert(
            new TeamMembershipReservation(Guid.NewGuid(), organizationId.Value, userId, teamId)
        );

        return Task.CompletedTask;
    }

    public async Task ReleaseAsync(OrganizationId organizationId, Guid userId, CancellationToken ct)
    {
        var reservation = await session
            .Query<TeamMembershipReservation>()
            .Where(r => r.OrganizationId == organizationId.Value && r.UserId == userId)
            .SingleOrDefaultAsync(ct);

        // Nothing to release is not an error: the rule is "at most one team", and a roster written
        // before this document existed has no row to delete.
        if (reservation == null)
            return;

        session.Delete(reservation);
    }
}
