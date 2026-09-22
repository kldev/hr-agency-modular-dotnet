using HrAgencySystem.Agency.Application.TimeSheets.RemoveWorkDay;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapRemoveWorkDay
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapDelete(ApiEndpoints.TimeSheets.MyDay, Handler)
            .WithSummary("Take a day off my sheet")
            .WithName("Remove work day")
            .ProducesStandardErrors()
            .Produces<WorkDayRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] DateOnly date,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<WorkDayRemoved>(
                new RemoveWorkDay(
                    user.OrganizationId,
                    user.UserId,
                    date.Year,
                    date.Month,
                    date,
                    user.UserId
                ),
                ct
            )
        );
}
