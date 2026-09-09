using HrAgencySystem.Company.Services;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeModuleService : ISalesService, IRecruitmentService, ICompanyService, IJobDescriptionService, IIdentityService
{
    public Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = new UserSnapshot(userId, "Test", "User", "test.user@test.io");

        return Task.FromResult(result);
    }

    public Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var suffix = companyId.ToString().Substring(4);
        var result = new CompanySnapshot(companyId, "Company  " + suffix,
            "TXT 101-200" + suffix);
        
        return Task.FromResult(result);
    }

    public Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task<JobApplicationInfo> GetApplicationAsync(Guid jobApplicationId, Guid organizationId, CancellationToken ct)
    {
        var result = new JobApplicationInfo(jobApplicationId, organizationId, Guid.NewGuid(), Guid.NewGuid());
        return Task.FromResult(result);
    }

    public Task<string> GetOrganizationSlug(OrganizationId organizationId, CancellationToken ct)
    {
        return Task.FromResult("Slug");
    }

    public Task<OpportunitySnapshot> GetOpportunityAsync(Guid organizationId, Guid opportunityId, CancellationToken ct)
    {
        var result = new OpportunitySnapshot(opportunityId, organizationId, Guid.NewGuid());
        return Task.FromResult(result);
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        // 
    }
}