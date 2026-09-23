using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.Endpoints;

internal static class Endpoint
{
    internal static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("").WithTags("Reports");

        Maps.MapRecruitment.Map(group);
        Maps.MapPlatform.Map(group);

        app.MapGet(ReportsServiceRoutes.Health, () => Results.Ok("healthy")).AllowAnonymous();
    }

    /// <summary>The period from the query string; a malformed one is a 400 via the exception handler.</summary>
    internal static ReportPeriod Period(string? from, string? to, TimeProvider clock) =>
        ReportPeriod.Create(from, to, DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime));
}
