using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.LegalEntity.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/legal-entities/{legalEntityId}
        endpoints
            .MapGet(ApiEndpoints.LegalEntities.Get, Handler)
            .WithSummary("Get legal entity")
            .WithName("Get legal entity")
            .Produces<LegalEntityProjection>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        ILegalEntitiesQueryRepository repository,
        [FromRoute] Guid legalEntityId,
        CancellationToken ct
    )
    {
        var result = await repository.GetLegalEntity(user.GetOrganization, legalEntityId, ct);

        if (result is null)
            return TypedResults.NotFound(
                DomainObjectNotFound.NotFound("Legal entity", legalEntityId)
            );

        return TypedResults.Ok(result);
    }
}
