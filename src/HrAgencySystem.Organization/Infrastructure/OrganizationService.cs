using HrAgencySystem.Organization.Events;
using HrAgencySystem.SharedKernel.Services;
using Marten;

namespace HrAgencySystem.Organization.Infrastructure;

public class OrganizationService(IQuerySession session) : IOrganizationService
{
    public async Task<IReadOnlyList<OrganizationInfo>> GetActiveOrganizationsAsync(CancellationToken ct)
    {
        return await session.Query<OrganizationCreated>()
            .Select(s => new OrganizationInfo(s.OrganizationId, s.Slug))
            .ToListAsync(ct);
    }
}