using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.LegalEntities.Application.Close;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

internal sealed record CloseLegalEntityRequest(DateOnly ActiveTo)
{
    internal CloseLegalEntity ToCommand(
        OrganizationId organizationId,
        Guid legalEntityId,
        Guid modifiedBy
    )
    {
        return new CloseLegalEntity(legalEntityId, organizationId.Value, ActiveTo, modifiedBy);
    }
}

internal static class MapClose
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/legal-entities/{legalEntityId}/close
        endpoints
            .MapPut(ApiEndpoints.LegalEntities.Close, Handler)
            .WithSummary("Close a legal entity")
            .WithName("Close legal entity")
            .Produces<LegalEntityClosed>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid legalEntityId,
        CloseLegalEntityRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<LegalEntityClosed>(
            request.ToCommand(user.GetOrganization, legalEntityId, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
