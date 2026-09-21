using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapGet(ApiEndpoints.Positions.Get, Handler)
            .WithSummary("Get a position")
            .WithName("Get position")
            .ProducesStandardErrors()
            .Produces<PositionDetails>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid positionId,
        IPositionsQueryRepository repository,
        CancellationToken ct
    )
    {
        var position =
            await repository.GetPosition(user.GetOrganization, positionId, ct)
            ?? throw new NotFoundException(nameof(PositionDetails), positionId);

        return TypedResults.Ok(position);
    }
}
