using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Services;

public sealed class AgencyService(IUserSnapshotRepository users, IOrganizationChecker checker)
    : IAgencyService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetUserAsync(userId, ct);

        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task<UserSnapshot> GetOrganizationMemberAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    )
    {
        var user = await users.GetUserAsync(userId, organizationId, ct);

        return user
            ?? throw new BusinessRuleException(IAgencyService.MemberNotInOrganizationMessage);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        if (!await checker.Exists(organizationId, ct))
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate is null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }
}
