using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Sales.Services;

public interface ISalesService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    Task<OpportunitySnapshot> GetOpportunityAsync(Guid organizationId, Guid opportunityId, CancellationToken ct);
    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);

}