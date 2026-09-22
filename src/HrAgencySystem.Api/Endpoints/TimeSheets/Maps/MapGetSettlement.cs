using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapGetSettlement
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.TimeSheets.Settlement, Handler)
            .RequireAuthorization(PayrollPolicy.Name)
            .WithSummary("Everything agreed for a month")
            .WithName("Get time sheets for settlement")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<TimeSheetProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimeSheetQueryRepository repository,
        CancellationToken ct,
        [FromQuery] int year,
        [FromQuery] int month
    ) =>
        TypedResults.Ok(
            await repository.GetForSettlementAsync(user.GetOrganization, year, month, ct)
        );
}
