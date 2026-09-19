using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Teams.Projections;

namespace HrAgencySystem.Teams.Application.Port;

public interface ITeamsQueryRepository
{
    Task<SliceResponse<TeamProjection>> GetTeams(
        OrganizationId organizationId,
        string search,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken ct
    );

    Task<TeamProjection?> GetTeam(OrganizationId organizationId, Guid teamId, CancellationToken ct);
}
