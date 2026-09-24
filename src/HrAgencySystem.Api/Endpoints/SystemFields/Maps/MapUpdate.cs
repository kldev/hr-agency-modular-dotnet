using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.SystemFields.Update;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SystemFields.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/system-fields/{id} - drafts follow the change, published versions keep their copy.
        endpoints
            .MapPut(ApiEndpoints.SystemFields.Update, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Change a system field's label, description, rules, options or source")
            .WithName("Update system field")
            .ProducesStandardErrors()
            .Produces<SystemFieldUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid systemFieldId,
        UpdateSystemFieldRequest request,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<SystemFieldUpdated>(
                request.ToCommand(systemFieldId, user.GetOrganization, user.UserId),
                ct
            )
        );

    internal sealed record UpdateSystemFieldRequest(
        string Label,
        string? Description,
        FieldRules? Rules,
        IReadOnlyList<ChoiceOption>? Options,
        SystemFieldSource Source
    )
    {
        public UpdateSystemField ToCommand(Guid systemFieldId, OrganizationId organizationId, Guid modifiedBy) =>
            new(organizationId.Value, systemFieldId, Label, Description, Rules, Options, Source, modifiedBy);
    }
}
