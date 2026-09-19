using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Documents;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Api.Endpoints.SalesFollowUpAction.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/sales/follow-up
        group
            .MapGet(ApiEndpoints.Sales.FollowUpActions.Slice, Handler)
            .WithSummary("Get follow up actions")
            .WithName("Get follow up actions")
            .Produces<SliceResponse<FollowUpAction>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IQueryFollowUpAction repository,
        AppUserAuthenticated user,
        Guid? opportunityId,
        Guid? companyId,
        int page = 1,
        int pageSize = 100,
        CancellationToken ct = default
    )
    {
        var query = new FollowUpActionQuery(opportunityId, companyId, page, pageSize);
        var result = await repository.GetSlicesAsync(user.OrganizationId, query, ct);

        return TypedResults.Ok(result);
    }
}
