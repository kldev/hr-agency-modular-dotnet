using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Tasks.Application.Reopen;
using HrAgencySystem.Tasks.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapReopen
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Tasks.Reopen, Handler)
            .WithSummary("Take a done task back to open")
            .WithName("Reopen task")
            .Produces<TaskItemReopened>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        [FromRoute] Guid taskId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TaskItemReopened>(new ReopenTaskItem(taskId, user.OrganizationId, user.UserId), ct)
        );
}
