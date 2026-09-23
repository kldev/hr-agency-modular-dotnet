using HrAgencySystem.ReportsService.Application.Export;
using HrAgencySystem.ReportsService.Application.Platform;
using HrAgencySystem.ReportsService.Auth;
using HrAgencySystem.ReportsService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.ReportsService.Endpoints.Maps;

internal static class MapPlatform
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ReportsServiceRoutes.Platform, Get)
            .RequireAuthorization(ReportPolicies.Platform);

        group
            .MapGet(ReportsServiceRoutes.PlatformExport, Export)
            .RequireAuthorization(ReportPolicies.Platform);
    }

    private static async Task<IResult> Get(
        PlatformReportQuery query,
        TimeProvider clock,
        [FromQuery(Name = ReportsServiceRoutes.FromQuery)] string? from,
        [FromQuery(Name = ReportsServiceRoutes.ToQuery)] string? to,
        CancellationToken ct
    )
    {
        var report = await query.RunAsync(Endpoint.Period(from, to, clock), ct);

        return Results.Ok(report);
    }

    private static async Task<IResult> Export(
        PlatformReportQuery query,
        TimeProvider clock,
        [FromQuery(Name = ReportsServiceRoutes.FromQuery)] string? from,
        [FromQuery(Name = ReportsServiceRoutes.ToQuery)] string? to,
        CancellationToken ct
    )
    {
        var report = await query.RunAsync(Endpoint.Period(from, to, clock), ct);

        return Results.File(
            ReportWorkbook.Platform(report),
            ReportFile.SpreadsheetContentType,
            ReportWorkbook.PlatformFileName(report)
        );
    }
}
