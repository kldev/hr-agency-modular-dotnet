using HrAgencySystem.Company.Documents;

namespace HrAgencySystem.Company.Application.Port;

public interface ICompanyContactQueryRepository
{
    Task<IReadOnlyList<CompanyContact>> GetAllAsync(Guid organizationId, Guid companyId, CancellationToken ct);
    Task<CompanyContact?> GetAsync(Guid organizationId, Guid contactId, CancellationToken ct);
}