using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/org-structure - the whole chart in one read, because every question about it
        // needs ancestors or descendants and there is nothing to page.
        endpoints
            .MapGet(ApiEndpoints.OrgStructure.Get, Handler)
            .WithSummary("Get the organizational structure")
            .WithName("Get org structure")
            .ProducesStandardErrors()
            .Produces<OrgStructureProjection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IOrgStructureQueryRepository repository,
        CancellationToken ct
    )
    {
        var structure = await repository.GetStructureAsync(user.GetOrganization, ct);

        // An organization that has never drawn its chart is not an error; it is a company on its
        // first day in the system.
        return TypedResults.Ok(structure);
    }
}
