using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Services;

public interface IJobDescriptionService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
    
}