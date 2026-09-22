using HrAgencySystem.Agency.Application.TimeSheets.Comment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.TimeSheets.Maps;

internal static class MapComment
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.TimeSheets.Comments, Handler)
            .WithSummary("Write on a sheet's thread")
            .WithName("Comment on time sheet")
            .ProducesStandardErrors()
            .Produces<TimeSheetCommented>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        [FromRoute] int year,
        [FromRoute] int month,
        CommentOnTimeSheetRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TimeSheetCommented>(
                new CommentOnTimeSheet(
                    user.OrganizationId,
                    userId,
                    year,
                    month,
                    request.Content,
                    PayrollPolicy.IsPayroll(user.Role),
                    user.UserId
                ),
                ct
            )
        );

    internal sealed record CommentOnTimeSheetRequest(string Content);
}
