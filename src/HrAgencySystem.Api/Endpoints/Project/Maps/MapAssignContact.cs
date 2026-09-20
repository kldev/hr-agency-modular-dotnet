using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Contacts.Assign;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapAssignContact
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/contacts/{role} - assigning replaces, so PUT rather than
        // POST: one person per role, and sending it twice leaves the same result.
        endpoints
            .MapPut(ApiEndpoints.Projects.AssignContact, Handler)
            .WithSummary("Assign project contact")
            .WithName("Assign project contact")
            .ProducesStandardErrors()
            .Produces<ProjectContactAssigned>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] ContactRole role,
        AssignProjectContactRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectContactAssigned>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                role: role,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record AssignProjectContactRequest(ContactPerson Person, Guid? CompanyContactId)
    {
        public AssignProjectContact ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            ContactRole role,
            Guid modifiedBy
        ) => new(projectId, organizationId.Value, role, Person, CompanyContactId, modifiedBy);
    }
}
