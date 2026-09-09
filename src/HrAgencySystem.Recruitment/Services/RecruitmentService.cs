using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Recruitment.Services;

public sealed class RecruitmentService(
    IUserSnapshotRepository userSnapshotRepository,
    ICompanySnapshotRepository companySnapshotRepository,
    IOrganizationChecker checker,
    IJobApplicationInfoQueryRepository applicationInfoQueryRepository ) : IRecruitmentService
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
    
    public async Task<JobApplicationInfo> GetApplicationAsync( 
        Guid jobApplicationId, Guid organizationId,
        CancellationToken ct)
    {
        var application = await applicationInfoQueryRepository.GetAsync(jobApplicationId, OrganizationId.From(organizationId), ct);
        return application ?? throw new NotFoundException("Job application", jobApplicationId);
    }
}