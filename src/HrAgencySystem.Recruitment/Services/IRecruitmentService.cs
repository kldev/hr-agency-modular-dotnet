using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Recruitment.Services;

public interface IRecruitmentService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    Task<JobApplicationInfo> GetApplicationAsync(
        Guid jobApplicationId, Guid organizationId,
        CancellationToken ct);
    
    public Task<string> GetOrganizationSlug(OrganizationId organizationId, CancellationToken ct);
}