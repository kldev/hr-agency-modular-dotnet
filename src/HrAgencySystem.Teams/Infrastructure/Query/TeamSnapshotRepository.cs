using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Projections;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Query;

public sealed class TeamSnapshotRepository(IDocumentSession session) : ITeamSnapshotRepository
{
    public async Task<TeamSnapshot?> GetTeamAsync(
        Guid teamId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<TeamProjection>()
            .Where(t => t.Id == teamId && t.OrganizationId == organizationId.Value)
            .Select(t => new TeamSnapshot(t.Id, t.Name))
            .FirstOrDefaultAsync(ct);

        if (result != null)
            return result;

        // The projection runs in the async daemon, so a team created moments ago may not be there
        // yet. The event carries the organization too, so the fallback stays scoped. Mirrors
        // UserSnapshotRepository, which has the same race for the same reason.
        return await session
            .Query<TeamCreated>()
            .Where(t => t.TeamId == teamId && t.OrganizationId == organizationId.Value)
            .Select(t => new TeamSnapshot(t.TeamId, t.Name))
            .FirstOrDefaultAsync(ct);
    }
}
