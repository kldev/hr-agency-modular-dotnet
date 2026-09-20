using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Contacts.Remove;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapRemoveContact
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/projects/{projectId}/contacts/{role}
        endpoints
            .MapDelete(ApiEndpoints.Projects.RemoveContact, Handler)
            .WithSummary("Remove project contact")
            .WithName("Remove project contact")
            .ProducesStandardErrors()
            .Produces<ProjectContactRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] ContactRole role,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectContactRemoved>(
            new RemoveProjectContact(
                projectId,
                user.GetOrganization.Value,
                role,
                user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
