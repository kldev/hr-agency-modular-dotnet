using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Projections;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public class SalesPipelineQueryRepository(IQuerySession session) : ISalesPipelineQueryRepository
{
    public async Task<IReadOnlyCollection<SalesPipelineQueryResult>> GetTotalsAsync(Guid organizationId,
        CancellationToken ct)
    {
        return await session.Query<OpportunityProjection>()
            .Where(x => x.OrganizationId == organizationId)
            .GroupBy(x => new { x.Stage, x.CurrencyCode })
            .Select(g => new SalesPipelineQueryResult(

                g.Key.Stage,
                g.Key.CurrencyCode,
                g.Count(),
                g.Sum(x => x.ExpectedValue)
            )).ToListAsync(ct);
    }
}