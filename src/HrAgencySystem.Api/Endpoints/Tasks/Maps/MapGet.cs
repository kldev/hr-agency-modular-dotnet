using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(ApiEndpoints.Tasks.Get, Handler)
            .WithSummary("Get a task")
            .WithName("Get task")
            .Produces<TaskItemProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        ITaskItemsQueryRepository repository,
        AppUserAuthenticated user,
        [FromRoute] Guid taskId,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await repository.GetTask(user.GetOrganization, taskId, ct)
                ?? throw new NotFoundException("Task", taskId)
        );
}
