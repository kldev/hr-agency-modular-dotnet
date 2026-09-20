using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.LegalEntities.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/legal-entities/{legalEntityId}
        endpoints
            .MapPut(ApiEndpoints.LegalEntities.Update, Handler)
            .WithSummary("Update legal entity")
            .WithName("Update legal entity")
            .Produces<LegalEntityUpdated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid legalEntityId,
        LegalEntityRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<LegalEntityUpdated>(
            request.ToUpdateCommand(user.GetOrganization, legalEntityId, user.UserId),
            ct
        );

        return TypedResults.Ok(result);
    }
}
