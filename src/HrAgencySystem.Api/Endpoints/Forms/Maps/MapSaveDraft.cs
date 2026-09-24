using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.FormDefinitions.SaveDraft;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Forms.Maps;

internal static class MapSaveDraft
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/forms/{formId}/draft - the whole working layout at once (plan 028 §3.3). A 400
        // carries fieldErrors keyed by page or field id, so the builder marks the right element.
        endpoints
            .MapPut(ApiEndpoints.Forms.Draft, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Save a form's working layout")
            .WithName("Save form draft")
            .ProducesStandardErrors()
            .Produces<FormDraftSaved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid formId,
        SaveFormDraftRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<FormDraftSaved>(
                request.ToCommand(formId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record SaveFormDraftRequest(
        [property: Description(
            "Every page with every field. A system field needs its systemFieldId and optionally a labelOverride; the rest of it is filled from the catalogue."
        )]
            IReadOnlyList<FormPage> Pages
    )
    {
        public SaveFormDraft ToCommand(Guid formId, OrganizationId organizationId, Guid modifiedBy) =>
            new(formId, organizationId.Value, Pages ?? [], modifiedBy);
    }
}
