using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/teams
        endpoints
            .MapGet(ApiEndpoints.Teams.Slice, Handler)
            .WithSummary("Get teams")
            .WithName("Get teams")
            .ProducesStandardErrors()
            .Produces<SliceResponse<TeamProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITeamsQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetTeams(
            user.GetOrganization,
            search ?? "",
            userId,
            page,
            pageSize,
            ct
        );

        return TypedResults.Ok(result);
    }
}
