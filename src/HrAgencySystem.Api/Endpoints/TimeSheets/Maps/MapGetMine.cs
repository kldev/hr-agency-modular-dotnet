using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapGetMine
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.TimeSheets.Mine, Handler)
            .WithSummary("Get my hours for a month")
            .WithName("Get my time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetProjection>();
    }

    /// <summary>
    /// A month nobody has written on yet answers 200 with nothing, not 404: an empty month is a
    /// normal state of affairs, and the sheet comes into being when the first day is saved.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimeSheetQueryRepository repository,
        CancellationToken ct,
        [FromQuery] int year,
        [FromQuery] int month
    ) =>
        TypedResults.Ok(
            await repository.GetAsync(user.GetOrganization, user.UserId, year, month, ct)
        );
}
