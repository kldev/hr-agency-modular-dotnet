using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.ChangeAssignmentStatus;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapChangeStatus
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/assignments/{assignmentId}/status - going live, finishing, breaking off, or
        // recording that somebody never turned up.
        endpoints
            .MapPut(ApiEndpoints.Assignments.ChangeStatus, Handler)
            .WithSummary("Change an assignment's status")
            .WithName("Change assignment status")
            .ProducesStandardErrors()
            .Produces<AssignmentStatusChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        ChangeAssignmentStatusRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentStatusChanged>(
            request.ToCommand(
                assignmentId: assignmentId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record ChangeAssignmentStatusRequest(
        [property: Description(
            "Planned, Active, Completed, Interrupted or DidNotStart. Active needs the person through the pipeline; Completed and Interrupted end a posting that ran, DidNotStart one that never did."
        )]
            AssignmentStatus Status,
        [property: Description(
            "The day it actually ended, for Completed or Interrupted; omit to keep the planned end. Ignored for other statuses."
        )]
            DateOnly? EndsOn = null,
        [property: Description(
            "Optional note on the change - worth giving for Interrupted and DidNotStart."
        )]
            string? Reason = null
    )
    {
        public ChangeAssignmentStatus ToCommand(
            Guid assignmentId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(assignmentId, organizationId.Value, Status, EndsOn, Reason, modifiedBy);
    }
}
