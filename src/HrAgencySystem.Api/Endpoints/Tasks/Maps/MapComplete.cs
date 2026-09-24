using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Tasks.Application.Complete;
using HrAgencySystem.Tasks.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapComplete
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Tasks.Complete, Handler)
            .WithSummary("Mark a task as done")
            .WithName("Complete task")
            .Produces<TaskItemCompleted>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        [FromRoute] Guid taskId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TaskItemCompleted>(new CompleteTaskItem(taskId, user.OrganizationId, user.UserId), ct)
        );
}
