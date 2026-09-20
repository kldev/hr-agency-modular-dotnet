using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Services;

public sealed class ProjectService(
    IUserSnapshotRepository users,
    ICompanySnapshotRepository companies,
    ITeamSnapshotRepository teams,
    IOrganizationChecker checker
) : IProjectService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetUserAsync(userId, ct);

        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        if (!await checker.Exists(organizationId, ct))
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public async Task<CompanySnapshot> GetCompanyAsync(
        OrganizationId organizationId,
        Guid companyId,
        CancellationToken ct
    )
    {
        var company = await companies.GetCompanyAsync(companyId, organizationId, ct);

        // A business rule, not a 404: answering "not found" would tell the caller whether that id
        // exists in somebody else's tenant.
        return company
            ?? throw new BusinessRuleException(IProjectService.CompanyNotInOrganizationMessage);
    }

    public async Task<TeamSnapshot> GetTeamAsync(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    )
    {
        var team = await teams.GetTeamAsync(teamId, organizationId, ct);

        return team
            ?? throw new BusinessRuleException(IProjectService.TeamNotInOrganizationMessage);
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate is null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }
}
