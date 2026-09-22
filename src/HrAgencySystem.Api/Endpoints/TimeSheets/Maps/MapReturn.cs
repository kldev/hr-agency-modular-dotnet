using HrAgencySystem.Agency.Application.TimeSheets.Return;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapReturn
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.TimeSheets.Return, Handler)
            .WithSummary("Send a month back to be corrected")
            .WithName("Return time sheet for correction")
            .ProducesStandardErrors()
            .Produces<TimeSheetReturnedForCorrection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        [FromRoute] int year,
        [FromRoute] int month,
        ReturnTimeSheetRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TimeSheetReturnedForCorrection>(
                new ReturnTimeSheetForCorrection(
                    user.OrganizationId,
                    userId,
                    year,
                    month,
                    request.Reason,
                    // Payroll may hand back what it has already been given; anybody else has to be
                    // above the person in the chart, which the handler checks for itself.
                    PayrollPolicy.IsPayroll(user.Role),
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record ReturnTimeSheetRequest(string Reason);
}
