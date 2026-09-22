using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Application.TimeSheets.Export;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapExportSettlement
{
    private const string SpreadsheetContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.TimeSheets.SettlementExport, Handler)
            .RequireAuthorization(RatesPolicy.Name)
            .WithSummary("The settlement month as a spreadsheet, with amounts")
            .WithName("Export time sheets for settlement")
            .ProducesStandardErrors()
            .Produces(StatusCodes.Status200OK, contentType: SpreadsheetContentType);
    }

    /// <summary>
    /// The same sheets the settlement list shows, so the file is a picture of that list rather
    /// than a second query that could answer differently. Downloading changes nothing: payroll
    /// makes the transfers and settles afterwards, and a failed transfer must not leave a month
    /// marked as done.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ITimeSheetQueryRepository sheets,
        IAgencyEmploymentQueryRepository employments,
        CancellationToken ct,
        [FromQuery] int year,
        [FromQuery] int month
    )
    {
        var agreed = await sheets.GetForSettlementAsync(user.GetOrganization, year, month, ct);

        var terms = await employments.GetForUsersAsync(
            user.GetOrganization,
            [.. agreed.Select(sheet => sheet.UserId)],
            ct
        );

        var bytes = SettlementWorkbook.Build(
            year,
            month,
            SettlementCalculator.Rows(agreed, terms)
        );

        return TypedResults.File(
            bytes,
            SpreadsheetContentType,
            SettlementWorkbook.FileName(year, month)
        );
    }
}
