using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using Marten;

namespace HrAgencySystem.Sales.Infrastructure.Persistence;

public class SalesPipelineQueryRepository(IQuerySession session) : ISalesPipelineQueryRepository
{
    public async Task<IReadOnlyCollection<SalesPipelineQueryResult>> GetTotalsAsync(Guid organizationId,
        OpportunityQuery query,
        CancellationToken ct)
    {
        return await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .WithOptionalCompanyId(query.CompanyId)
            .WithResponsibleId(query.ResponsibleId)
            .WithStage(query.Stage)
            .WithSearch(query.Search)
            .GroupBy(x => new { x.Stage, x.CurrencyCode })
            .Select(g => new SalesPipelineQueryResult(

                g.Key.Stage,
                g.Key.CurrencyCode,
                g.Count(),
                g.Sum(x => x.ExpectedValue)
            )).ToListAsync(ct);
    }

    public  async Task<IReadOnlyCollection<SalesPipelineResponsibleQueryResult>> GetResponsibleTotalsAsync(Guid organizationId, CancellationToken ct)
    {
        var result = await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .GroupBy(x => new { x.Stage, x.CurrencyCode, x.ResponsibleId })
            .Select(g => new SalesPipelineResponsibleQueryResult(
                g.Key.Stage,
                g.Key.CurrencyCode,
                g.Key.ResponsibleId,
                g.Count(),
                g.Sum(x => x.ExpectedValue),
                null
            )).ToListAsync(ct);

        var usersIds = result.Select(z => z.ResponsibleId).Distinct().ToList();
        var users = await session.Query<OpportunityProjection>()
            .WithOrganizationId(organizationId)
            .WithResponsibleIds(usersIds)
            .Select(z => z.Responsible)
            .Distinct()
            .ToListAsync(ct);

        var joinedResult = result.Select(z => z with
        {
            Responsible = users.SingleOrDefault(r=>r.Id == z.ResponsibleId)
        }).OrderBy(z=>z.Responsible?.LastName).ToList();
        
        return joinedResult;
    }
}