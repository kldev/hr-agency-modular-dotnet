using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Identity.Application.ApiKeys.Revoke;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Owner.Maps;

internal static class MapRevokeApiKey
{
    internal static void Map(RouteGroupBuilder group)
    {
        group
            .MapDelete(ApiEndpoints.Owners.ApiKey, Handler)
            .WithSummary("Revoke a key; the row stays so its history does")
            .WithName("Revoke service API key")
            .Produces<ServiceApiKeyRevoked>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        OwnerAuthenticated owner,
        [FromRoute] Guid keyId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<ServiceApiKeyRevoked>(new RevokeServiceApiKey(keyId, owner.Id), ct)
        );
}
