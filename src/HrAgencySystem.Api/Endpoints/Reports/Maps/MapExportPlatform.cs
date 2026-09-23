using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.ReportsService.Contracts;
using HrAgencySystem.SharedKernel.Time;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Reports.Maps;

internal static class MapExportPlatform
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Reports.PlatformExport, Handler)
            .WithSummary("The platform report as a spreadsheet - platform owner only")
            .WithName("Export platform report")
            .ProducesStandardErrors()
            .Produces(StatusCodes.Status200OK, contentType: ReportFile.SpreadsheetContentType)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);
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
        var file = await reports.ExportPlatformAsync(
            owner.Id,
            Endpoint.Period(from, to, clock),
            ct
        );

        return TypedResults.File(file.Content, file.ContentType, file.FileName);
    }
}
