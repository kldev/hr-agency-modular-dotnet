using System.ComponentModel;
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
                    user.UserId,
                    request.Comment
                ),
                ct
            )
        );

    internal sealed record SubmitTimeSheetRequest(
        [property: Description("Year of the month being sent.")] int Year,
        [property: Description("Month being sent, 1 to 12. A month with no hours cannot be sent.")]
            int Month,
        [property: Description("Optional note to the supervisor that travels with it.")]
            string? Comment = null
    );
}
