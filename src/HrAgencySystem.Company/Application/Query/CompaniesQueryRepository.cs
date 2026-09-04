using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Company.Application.Query;

public sealed class CompaniesQueryRepository(IQuerySession session)
    : ICompaniesQueryRepository
{
    public Task<SliceResponse<CompanyProjection>> GetCompanies(
        string search,
        Guid organizationId,
        int page = 1,
        int pageSize = 10)
    {
        var query = session.Query<CompanyProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id);

        return query.ToSlice(page, pageSize);
    }

    public async Task<CompanyProjection?> GetCompany(Guid organizationId, Guid? companyId, string taxId, CancellationToken ct)
    {
        return await session.Query<CompanyProjection>()
            .WithOrganizationId(organizationId)
            .WithCompanyId(companyId)
            .WithTax(taxId).SingleOrDefaultAsync(ct);
    }
}