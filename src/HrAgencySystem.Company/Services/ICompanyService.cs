using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Company.Services;

public interface ICompanyService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    
}