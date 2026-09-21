using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapGetSupervisor
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/org-structure/supervisor/{userId} - the question leave and timesheets are
        // waiting on. Computed from the chart on every read, never stored on the person.
        endpoints
            .MapGet(ApiEndpoints.OrgStructure.Supervisor, Handler)
            .WithSummary("Get who answers for this person")
            .WithName("Get supervisor")
            .ProducesStandardErrors()
            .Produces<SupervisorView>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        IOrgStructureQueryRepository repository,
        CancellationToken ct
    )
    {
        var supervisor = await repository.GetSupervisorAsync(user.GetOrganization, userId, ct);

        // Null is an answer, not a miss: the person at the top has nobody above them, and a 404
        // would read as "we do not know this person".
        return TypedResults.Ok(supervisor);
    }
}
