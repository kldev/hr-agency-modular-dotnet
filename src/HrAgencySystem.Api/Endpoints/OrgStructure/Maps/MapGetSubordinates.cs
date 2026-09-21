using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.OrgStructure.Maps;

internal static class MapGetSubordinates
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.OrgStructure.Subordinates, Handler)
            .WithSummary("Get everybody this person answers for")
            .WithName("Get subordinates")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<Guid>>();
    }

    /// <summary>
    /// <paramref name="wholeSubtree"/> is the difference between "my own people" and "everybody
    /// under me" - payroll wants the second, a head approving leave wants the first.
    /// </summary>
    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid userId,
        IOrgStructureQueryRepository repository,
        [FromQuery] bool wholeSubtree = false,
        CancellationToken ct = default
    ) =>
        TypedResults.Ok(
            await repository.GetSubordinatesAsync(user.GetOrganization, userId, wholeSubtree, ct)
        );
}
