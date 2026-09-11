using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Identity.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class IdentityService( 
    IUserSnapshotRepository userSnapshotRepository, 
    IOrganizationChecker checker,
    IQueryOrganizationRepository organizationRepository) : IIdentityService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty) return new UserSnapshot(Guid.NewGuid(), "", "", "system");
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public async Task<OrganizationInfo> GetOrganization(OrganizationId organizationId, CancellationToken ct)
    {
        var info = await organizationRepository.GetOrganization(organizationId, ct);
        return info ?? throw new NotFoundException("Organization", organizationId.Value);
    }

    public async Task<string> GetOrganizationSlug(OrganizationId organizationId, CancellationToken ct)
    {
        var slug = await checker.GetSlug(organizationId.Value, ct);
        return slug ?? throw new NotFoundException("Organization", organizationId.Value);
    }
}