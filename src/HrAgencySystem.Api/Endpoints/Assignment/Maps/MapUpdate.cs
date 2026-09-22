using System.ComponentModel;
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
        [property: Description(
            "The role in the same project. Changing it moves the person's seat from one role to the other; the project itself never changes - moving somebody ends one assignment and plans another."
        )]
            Guid PositionId,
        [property: Description("First day of the posting.")] DateOnly StartsOn,
        [property: Description("Last day of the posting, or null for open-ended.")]
            DateOnly? EndsOn = null
    )
    {
        public UpdateAssignment ToCommand(
            Guid assignmentId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(assignmentId, organizationId.Value, PositionId, StartsOn, EndsOn, modifiedBy);
    }
}
