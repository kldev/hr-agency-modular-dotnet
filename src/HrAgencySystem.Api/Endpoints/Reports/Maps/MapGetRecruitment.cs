using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Reports.Maps;

internal static class MapGetRecruitment
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Reports.Recruitment, Handler)
            .WithSummary("The organization's recruitment funnel and monthly activity")
            .WithName("Get recruitment report")
            .Produces<RecruitmentReport>()
            .ProducesStandardErrors()
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IReportsClient reports,
        IClock clock,
        CancellationToken ct,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null
    )
    {
        var report = await reports.GetRecruitmentAsync(
            user.OrganizationId,
            user.UserId,
            Endpoint.Period(from, to, clock),
            ct
        );

        return TypedResults.Ok(report);
    }
}
