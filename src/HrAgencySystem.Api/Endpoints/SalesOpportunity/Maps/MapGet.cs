using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Sales.Application.Queries;

namespace HrAgencySystem.Api.Endpoints.SalesOpportunity.Maps;


internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/sales/opportunity/{opportunityId}
        group.MapGet("{opportunityId:guid}", Handler).WithSummary("Get opportunity");
    }

    private static async Task<IResult> Handler(
        IOpportunityQueryRepository repository,
        AppUserAuthenticated user,
        Guid opportunityId,
        CancellationToken ct
    )
    {
        var result = await repository
            .GetByIdAsync(user.OrganizationId, opportunityId, ct);

        if (result is null)
            return TypedResults.NotFound(
                DomainObjectNotFound.NotFound("Sales opportunity", opportunityId));

        return TypedResults.Ok(result);
    }

}