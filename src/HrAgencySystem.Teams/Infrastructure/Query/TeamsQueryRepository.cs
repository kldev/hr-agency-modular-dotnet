using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Projections;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Query;

public sealed class TeamsQueryRepository(IDocumentSession session) : ITeamsQueryRepository
{
    public async Task<SliceResponse<TeamProjection>> GetTeams(
        OrganizationId organizationId,
        string search,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken ct
    )
    {
        var query = session
            .Query<TeamProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithMember(userId)
            .OrderBy(t => t.Name);

        return await query.ToSlice(page, pageSize, ct);
    }

    public async Task<TeamProjection?> GetTeam(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    )
    {
        return await session
            .Query<TeamProjection>()
            .WithOrganizationId(organizationId)
            .WithTeamId(teamId)
            .FirstOrDefaultAsync(ct);
    }
}
