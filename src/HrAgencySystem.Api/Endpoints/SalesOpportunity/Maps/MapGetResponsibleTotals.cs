using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Queries;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapGetResponsibleTotals
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/sales/opportunity/totals-responsible
        group.MapGet("totals-responsible", Handler)
            .WithSummary("Get responsible totals")
            .WithName("Get opportunities responsible totals")
            .Produces<IReadOnlyList<SalesPipelineResponsibleQueryResult>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        ISalesPipelineQueryRepository repository, 
        AppUserAuthenticated user,
        CancellationToken ct = default)
    {
        
        var result = await repository.GetResponsibleTotalsAsync(user.OrganizationId, ct);

        return TypedResults.Ok(result);
    }
}