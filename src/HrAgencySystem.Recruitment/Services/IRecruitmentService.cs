using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Recruitment.Services;

public interface IRecruitmentService
{
    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);
    Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct);
    Task ValidateOrganization(Guid organizationId, CancellationToken ct);
}