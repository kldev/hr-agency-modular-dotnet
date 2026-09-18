using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public sealed class QueryFollowUpAction(IQuerySession session) : IQueryFollowUpAction
{
    public async Task<SliceResponse<FollowUpAction>> GetSlicesAsync(Guid organizationId,
        FollowUpActionQuery query, CancellationToken ct)
    {
        return await session.Query<FollowUpAction>()
            .WithOrganizationId(organizationId)
            .WithOpportunityId(query.OpportunityId)
            .WithCompanyId(query.CompanyId)
            .OrderByDescending(z => z.FollowDateTime)
            .ToSlice(query, ct);
    }

    public async Task<FollowUpAction?> GetByIdAsync(Guid organizationId, Guid followUpActionId,
        CancellationToken ct)
    {
        return await session.Query<FollowUpAction>()
            .WithOrganizationId(organizationId)
            .Where(z => z.Id == followUpActionId)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<FollowUpAction?> GetLatestAsync(Guid organizationId, Guid opportunityId,
        CancellationToken ct)
    {
        return await session.Query<FollowUpAction>()
            .WithOrganizationId(organizationId)
            .WithOpportunityId(opportunityId)
            .OrderByDescending(z => z.FollowDateTime)
            .FirstOrDefaultAsync(ct);
    }
}
