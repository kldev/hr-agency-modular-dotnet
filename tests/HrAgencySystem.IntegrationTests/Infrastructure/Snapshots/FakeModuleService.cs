using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.IntegrationTests.Infrastructure.Snapshots;

public class FakeModuleService : ISalesService, IRecruitmentService
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

    public Task<OpportunitySnapshot> GetOpportunityAsync(Guid organizationId, Guid opportunityId, CancellationToken ct)
    {
        var result = new OpportunitySnapshot(opportunityId, organizationId, Guid.NewGuid());
        return Task.FromResult(result);
    }
}