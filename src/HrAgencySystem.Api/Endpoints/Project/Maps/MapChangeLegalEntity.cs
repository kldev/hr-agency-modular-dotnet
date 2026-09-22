using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.LegalEntity.Change;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal sealed record ChangeProjectLegalEntityRequest(
    [property: Description("The agency's own company that takes over delivering the project.")]
        Guid LegalEntityId
)
{
    internal ChangeProjectLegalEntity ToCommand(
        Guid projectId,
        OrganizationId organizationId,
        Guid modifiedBy
    )
    {
        return new ChangeProjectLegalEntity(
            projectId,
            organizationId.Value,
            LegalEntityId,
            modifiedBy
        );
    }
}

internal static class MapChangeLegalEntity
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/legal-entity
        endpoints
            .MapPut(ApiEndpoints.Projects.ChangeLegalEntity, Handler)
            .WithSummary("Change the delivering legal entity")
            .WithName("Change project legal entity")
            .Produces<ProjectLegalEntityChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        ChangeProjectLegalEntityRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectLegalEntityChanged>(
            request.ToCommand(projectId, user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
