using System.ComponentModel;
using HrAgencySystem.Agency.Application.TimeSheets.Approve;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapApprove
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // No role policy here on purpose: who may approve follows from the chart, and a role that
        // could approve anybody's hours would make the structure decorative.
        endpoints
            .MapPost(ApiEndpoints.TimeSheets.Approve, Handler)
            .WithSummary("Accept somebody's month")
            .WithName("Approve time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetApproved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        [FromRoute] int year,
        [FromRoute] int month,
        ApproveTimeSheetRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TimeSheetApproved>(
                new ApproveTimeSheet(
                    user.OrganizationId,
                    userId,
                    year,
                    month,
                    request.Comment,
                    user.UserId
                ),
                ct
            )
        );

    /// <summary>A note is optional here; on a return it is not.</summary>
    internal sealed record ApproveTimeSheetRequest(
        [property: Description(
            "Optional note to the person. Only their supervisor from the chart may approve."
        )]
            string? Comment
    );
}
