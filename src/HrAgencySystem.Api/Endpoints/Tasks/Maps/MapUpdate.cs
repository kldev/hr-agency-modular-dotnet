using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Tasks.Application.Update;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(ApiEndpoints.Tasks.Update, Handler)
            .WithSummary("Change an open task")
            .WithName("Update task")
            .Produces<TaskItemUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        [FromRoute] Guid taskId,
        UpdateTaskRequest request,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<TaskItemUpdated>(
                request.ToCommand(taskId, user.OrganizationId, user.UserId),
                ct
            )
        );
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record UpdateTaskRequest(
    [property: Description("One of the task company's opportunities, or none.")] Guid? OpportunityId,
    [property: Description("What has to be done, up to 200 characters.")] string Title,
    [property: Description("Optional details, up to 2000 characters.")] string? Description,
    [property: Description("When it has to be done by.")] DateTimeOffset DueAt,
    [property: Description("Low, Medium or High.")] TaskPriority Priority,
    [property: Description("Whose task it is.")] Guid AssigneeId
)
{
    public UpdateTaskItem ToCommand(Guid taskId, Guid organizationId, Guid modifiedBy) =>
        new(
            taskId,
            organizationId,
            modifiedBy,
            OpportunityId,
            Title,
            Description,
            DueAt,
            Priority,
            AssigneeId
        );
}
