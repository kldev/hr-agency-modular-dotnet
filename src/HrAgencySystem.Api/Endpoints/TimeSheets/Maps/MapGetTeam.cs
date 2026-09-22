using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapGetTeam
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // The screen somebody opens daily. Approval happens once a month; chasing people does not.
        endpoints
            .MapGet(ApiEndpoints.TimeSheets.Team, Handler)
            .WithSummary("Monitor everybody below me")
            .WithName("Get team time sheets")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<TeamTimeSheetRow>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimeSheetQueryRepository repository,
        CancellationToken ct,
        [FromQuery] int year,
        [FromQuery] int month
    ) =>
        TypedResults.Ok(
            await repository.GetTeamAsync(user.GetOrganization, user.UserId, year, month, ct)
        );
}
