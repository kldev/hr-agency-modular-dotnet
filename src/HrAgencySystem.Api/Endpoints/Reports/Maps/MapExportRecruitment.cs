using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Reports.Maps;

internal static class MapExportRecruitment
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Reports.RecruitmentExport, Handler)
            .WithSummary("The recruitment report as a spreadsheet")
            .WithName("Export recruitment report")
            .ProducesStandardErrors()
            .Produces(StatusCodes.Status200OK, contentType: ReportFile.SpreadsheetContentType)
            .Produces(StatusCodes.Status503ServiceUnavailable);
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
        var file = await reports.ExportRecruitmentAsync(
            user.OrganizationId,
            user.UserId,
            Endpoint.Period(from, to, clock),
            ct
        );

        return TypedResults.File(file.Content, file.ContentType, file.FileName);
    }
}
