using HrAgencySystem.Agency.Application.TimeSheets.Settle;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapSettle
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.TimeSheets.Settle, Handler)
            .RequireAuthorization(PayrollPolicy.Name)
            .WithSummary("Hand an approved month to payroll")
            .WithName("Settle time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetSettled>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        [FromRoute] int year,
        [FromRoute] int month,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TimeSheetSettled>(
                new SettleTimeSheet(
                    user.OrganizationId,
                    userId,
                    year,
                    month,
                    PayrollPolicy.IsPayroll(user.Role),
                    user.UserId
                ),
                ct
            )
        );
}
