using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Documents;

namespace HrAgencySystem.Api.Endpoints.SalesFollowUpAction.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/sales/follow-up/{followUpActionId}
        group
            .MapGet("{followUpActionId:guid}", Handler)
            .WithSummary("Get follow up action")
            .WithName("Get follow up action")
            .Produces<FollowUpAction>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IQueryFollowUpAction repository,
        AppUserAuthenticated user,
        Guid followUpActionId,
        CancellationToken ct
    )
    {
        var result = await repository.GetByIdAsync(user.OrganizationId, followUpActionId, ct);

        if (result is null)
            return TypedResults.NotFound(
                DomainObjectNotFound.NotFound("Follow up action", followUpActionId)
            );

        return TypedResults.Ok(result);
    }
}
