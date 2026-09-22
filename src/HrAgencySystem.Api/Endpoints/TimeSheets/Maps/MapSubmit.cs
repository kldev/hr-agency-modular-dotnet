using HrAgencySystem.Agency.Application.TimeSheets.Submit;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapSubmit
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.TimeSheets.MySubmit, Handler)
            .WithSummary("Send my month for approval")
            .WithName("Submit time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetSubmitted>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        SubmitTimeSheetRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TimeSheetSubmitted>(
                new SubmitTimeSheet(
                    user.OrganizationId,
                    user.UserId,
                    request.Year,
                    request.Month,
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record SubmitTimeSheetRequest(int Year, int Month);
}
