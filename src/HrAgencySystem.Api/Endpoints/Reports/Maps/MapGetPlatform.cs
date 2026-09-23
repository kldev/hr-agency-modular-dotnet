using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Reports.Maps;

internal static class MapGetPlatform
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Reports.Platform, Handler)
            .WithSummary("Every organization's activity side by side - platform owner only")
            .WithName("Get platform report")
            .Produces<PlatformReport>()
            .ProducesStandardErrors()
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated owner,
        IReportsClient reports,
        IClock clock,
        CancellationToken ct,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null
    )
    {
        var report = await reports.GetPlatformAsync(owner.Id, Endpoint.Period(from, to, clock), ct);

        return TypedResults.Ok(report);
    }
}
