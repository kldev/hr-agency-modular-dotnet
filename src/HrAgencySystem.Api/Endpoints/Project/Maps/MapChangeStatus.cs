using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.ChangeStatus;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapChangeStatus
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/status
        endpoints
            .MapPut(ApiEndpoints.Projects.ChangeStatus, Handler)
            .WithSummary("Change project status")
            .WithName("Change project status")
            .ProducesStandardErrors()
            .Produces<ProjectStatusChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        ChangeProjectStatusRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectStatusChanged>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record ChangeProjectStatusRequest(
        [property: Description(
            "Draft, Active, Suspended, Completed or Cancelled. Going Active needs a signed contract, a responsible contact and a complete client profile; Completed and Cancelled are final."
        )]
            ProjectStatus Status,
        [property: Description(
            "Optional note on why, up to 500 characters - worth giving for Suspended and Cancelled."
        )]
            string? Reason
    )
    {
        public ChangeProjectStatus ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(projectId, organizationId.Value, Status, Reason, modifiedBy);
    }
}
