using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using JetBrains.Annotations;

namespace HrAgencySystem.Company.Services;

[UsedImplicitly] // Registered in the container; handlers receive it as ICompanyService.
public sealed class CompanyService(
    IUserSnapshotRepository userSnapshotRepository,
    IOrganizationChecker checker
) : ICompanyService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
}
