using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapUpdateDetails
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPut(ApiEndpoints.Forms.UpdateDetails, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Change a form's name, description and kind")
            .WithName("Update form details")
            .ProducesStandardErrors()
            .Produces<FormDetailsUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid formId,
        UpdateFormDetailsRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormDetailsUpdated>(
                request.ToCommand(formId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record UpdateFormDetailsRequest(string Name, string? Description, FormKind Kind)
    {
        public UpdateFormDetails ToCommand(Guid formId, OrganizationId organizationId, Guid modifiedBy) =>
            new(formId, organizationId.Value, Name, Description, Kind, modifiedBy);
    }
}
