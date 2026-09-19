using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Teams.Services;

public sealed class TeamsService(
    IUserSnapshotRepository userSnapshotRepository,
    IOrganizationChecker checker
) : ITeamsService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);

        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task<UserSnapshot> GetOrganizationMemberAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    )
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, organizationId, ct);

        return user
            ?? throw new BusinessRuleException(ITeamsService.MemberNotInOrganizationMessage);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var exists = await checker.Exists(organizationId, ct);

        if (!exists)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate == null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }
}
