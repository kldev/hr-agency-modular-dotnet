using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Services;

public interface ISalesService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    Task<OpportunitySnapshot> GetOpportunityAsync(Guid organizationId, Guid opportunityId, CancellationToken ct);
}