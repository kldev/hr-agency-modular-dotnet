using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapPlan
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/assignments - put one person on one project for one period. Moving somebody
        // between projects is two calls, this one and ending the previous assignment; there is no
        // endpoint that repoints an existing one, on purpose.
        endpoints
            .MapPost(ApiEndpoints.Assignments.Plan, Handler)
            .WithSummary("Plan an assignment")
            .WithName("Plan assignment")
            .ProducesStandardErrors()
            .Produces<AssignmentPlanned>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        PlanAssignmentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentPlanned>(
            request.ToCommand(organizationId: user.GetOrganization, createdBy: user.UserId),
            ct
        );

        return TypedResults.Created($"/api/assignments/{result.AssignmentId}", result);
    }

    internal sealed record PlanAssignmentRequest(
        Guid WorkerId,
        Guid ProjectId,
        EngagementType EngagementType,
        /// <summary>A role opened in that project - the name is the position's, not this request's.</summary>
        Guid PositionId,
        DateOnly StartsOn,
        DateOnly? EndsOn = null
    )
    {
        public PlanAssignment ToCommand(OrganizationId organizationId, Guid createdBy) =>
            new(
                organizationId.Value,
                WorkerId,
                ProjectId,
                EngagementType,
                PositionId,
                StartsOn,
                EndsOn,
                createdBy
            );
    }
}
