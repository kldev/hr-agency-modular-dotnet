using HrAgencySystem.ReportsService.Application.Export;
using HrAgencySystem.ReportsService.Application.Recruitment;
using HrAgencySystem.ReportsService.Auth;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.ReportsService.Endpoints.Maps;

internal static class MapRecruitment
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ReportsServiceRoutes.Recruitment, Get)
            .RequireAuthorization(ReportPolicies.Organization);

        group
            .MapGet(ReportsServiceRoutes.RecruitmentExport, Export)
            .RequireAuthorization(ReportPolicies.Organization);
    }

    private static async Task<IResult> Get(
        OrganizationCaller caller,
        RecruitmentReportQuery query,
        TimeProvider clock,
        [FromQuery(Name = ReportsServiceRoutes.FromQuery)] string? from,
        [FromQuery(Name = ReportsServiceRoutes.ToQuery)] string? to,
        CancellationToken ct
    )
    {
        var report = await query.RunAsync(
            caller.OrganizationId,
            Endpoint.Period(from, to, clock),
            ct
        );

        return Results.Ok(report);
    }

    private static async Task<IResult> Export(
        OrganizationCaller caller,
        RecruitmentReportQuery query,
        TimeProvider clock,
        [FromQuery(Name = ReportsServiceRoutes.FromQuery)] string? from,
        [FromQuery(Name = ReportsServiceRoutes.ToQuery)] string? to,
        CancellationToken ct
    )
    {
        var report = await query.RunAsync(
            caller.OrganizationId,
            Endpoint.Period(from, to, clock),
            ct
        );

        return Results.File(
            ReportWorkbook.Recruitment(report),
            ReportFile.SpreadsheetContentType,
            ReportWorkbook.RecruitmentFileName(report)
        );
    }
}
