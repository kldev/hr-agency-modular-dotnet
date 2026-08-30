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
            .Where(c => c.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        query = query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id);

        return query.ToSlice(page, pageSize);
    }
}