using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Positions.Restore;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapRestore
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.Projects.RestorePosition, Handler)
            .WithSummary("Restore a position")
            .WithName("Restore position")
            .ProducesStandardErrors()
            .Produces<ProjectPositionRestored>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid positionId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<ProjectPositionRestored>(
                new RestorePosition(
                    projectId,
                    user.GetOrganization.Value,
                    positionId,
                    user.UserId
                ),
                ct
            )
        );
}
