using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/sales/opportunity
        group.MapGet("", Handler)
            .WithSummary("Get opportunities")
            .WithName("Get opportunities")
            .Produces<SliceResponse<OpportunityProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IOpportunityQueryRepository repository, 
        AppUserAuthenticated user,
        string? search,
        Guid? companyId,
        Guid? responsibleId,
        OpportunityStage? stage,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new OpportunityQuery(search ?? "", companyId, responsibleId, stage, page, pageSize);
        var result = await repository.GetSlicesAsync(user.OrganizationId, query, ct);

        return TypedResults.Ok(result);
    }
}