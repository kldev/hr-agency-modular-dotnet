using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Tasks.Application.Create;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Tasks.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(ApiEndpoints.Tasks.Create, Handler)
            .WithSummary("Create a task")
            .WithName("Create task")
            .Produces<TaskItemCreated>(StatusCodes.Status201Created)
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        IMessageBus bus,
        AppUserAuthenticated user,
        CreateTaskRequest request,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<TaskItemCreated>(
            request.ToCommand(user.OrganizationId, user.UserId),
            ct
        );

        return TypedResults.Created($"/api/tasks/{result.TaskId}", result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record CreateTaskRequest(
    [property: Description("The client company the task is for. Required.")] Guid CompanyId,
    [property: Description("One of that company's opportunities, when the task belongs to a deal.")]
        Guid? OpportunityId,
    [property: Description("What has to be done, up to 200 characters.")] string Title,
    [property: Description("Optional details, up to 2000 characters.")] string? Description,
    [property: Description("When it has to be done by.")] DateTimeOffset DueAt,
    [property: Description("Low, Medium or High.")] TaskPriority Priority,
    [property: Description("Whose task it is; the caller's own when left out.")] Guid? AssigneeId
)
{
    public CreateTaskItem ToCommand(Guid organizationId, Guid createdBy) =>
        new(
            organizationId,
            createdBy,
            CompanyId,
            OpportunityId,
            Title,
            Description,
            DueAt,
            Priority,
            AssigneeId
        );
}
