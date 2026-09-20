using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Emails.Set;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapSetEmails
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/emails/{purpose} - the whole set for one purpose, replaced.
        endpoints
            .MapPut(ApiEndpoints.Projects.SetEmails, Handler)
            .WithSummary("Set project email recipients")
            .WithName("Set project email recipients")
            .ProducesStandardErrors()
            .Produces<ProjectEmailRecipientsChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] EmailPurpose purpose,
        SetProjectEmailRecipientsRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ProjectEmailRecipientsChanged>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                purpose: purpose,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record SetProjectEmailRecipientsRequest(IReadOnlyList<string> Emails)
    {
        public SetProjectEmailRecipients ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            EmailPurpose purpose,
            Guid modifiedBy
        ) => new(projectId, organizationId.Value, purpose, Emails, modifiedBy);
    }
}
