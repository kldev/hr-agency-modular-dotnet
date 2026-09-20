using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.LegalEntities.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

internal static class MapCreate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/legal-entities
        endpoints
            .MapPost(ApiEndpoints.LegalEntities.Create, Handler)
            .WithSummary("Create legal entity")
            .WithName("Create legal entity")
            .Produces<LegalEntityCreated>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        LegalEntityRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<LegalEntityCreated>(
            request.ToCreateCommand(user.GetOrganization, user.UserId),
            ct
        );

        return TypedResults.Created($"/api/legal-entities/{result.LegalEntityId}", result);
    }
}
