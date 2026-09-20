using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.UpdateAssignment;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/assignments/{assignmentId} - the position and the period. The project, the
        // person and the engagement type are not in the body because they cannot change.
        endpoints
            .MapPut(ApiEndpoints.Assignments.Update, Handler)
            .WithSummary("Update an assignment")
            .WithName("Update assignment")
            .ProducesStandardErrors()
            .Produces<AssignmentUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        UpdateAssignmentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentUpdated>(
            request.ToCommand(
                assignmentId: assignmentId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record UpdateAssignmentRequest(
        string Position,
        DateOnly StartsOn,
        DateOnly? EndsOn = null
    )
    {
        public UpdateAssignment ToCommand(
            Guid assignmentId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(assignmentId, organizationId.Value, Position, StartsOn, EndsOn, modifiedBy);
    }
}
