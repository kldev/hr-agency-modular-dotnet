using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Services;

public sealed class JobDescriptionService(
    IUserSnapshotRepository userSnapshotRepository,
    ICompanySnapshotRepository companySnapshotRepository,
    IOrganizationChecker checker) : IJobDescriptionService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var company = await companySnapshotRepository.GetCompanyAsync(companyId, ct);
        return company ?? throw new NotFoundException("Company", companyId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
}