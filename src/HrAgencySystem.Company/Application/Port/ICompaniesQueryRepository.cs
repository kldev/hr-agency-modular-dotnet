using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Company.Application.Port;

public interface ICompaniesQueryRepository
{
    Task<SliceResponse<CompanyProjection>> GetCompanies(string search, Guid organizationId, int page, int pageSize);
    Task<CompanyProjection?> GetCompany(Guid organizationId, Guid? companyId, string taxId, CancellationToken ct);
}