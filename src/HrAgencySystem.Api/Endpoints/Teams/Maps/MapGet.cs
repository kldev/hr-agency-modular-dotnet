using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Teams.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/teams/{teamId}
        endpoints
            .MapGet(ApiEndpoints.Teams.Get, Handler)
            .WithSummary("Get team")
            .WithName("Get team")
            .ProducesStandardErrors()
            .Produces<TeamProjection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITeamsQueryRepository repository,
        [FromRoute] Guid teamId,
        CancellationToken ct
    )
    {
        var result =
            await repository.GetTeam(user.GetOrganization, teamId, ct)
            ?? throw new NotFoundException("Team", teamId);

        return TypedResults.Ok(result);
    }
}
