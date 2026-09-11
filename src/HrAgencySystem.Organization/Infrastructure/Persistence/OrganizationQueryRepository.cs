using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure.Persistence;

public class OrganizationQueryRepository(IQuerySession session) : IOrganizationQueryRepository
{
    public async Task<SliceResponse<OrganizationProjection>> GetSlice(string? search, int page, int pageSize, CancellationToken ct)
    {
        return await session.Query<OrganizationProjection>()
            .WithSearch(search)
            .ToSlice(page, pageSize, ct);
    }
}