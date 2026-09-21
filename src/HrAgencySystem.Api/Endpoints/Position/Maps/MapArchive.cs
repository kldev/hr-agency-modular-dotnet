using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Positions.Archive;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Position.Maps;

internal static class MapArchive
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST .../archive - closing a role, which is as far as removing one goes here.
        endpoints
            .MapPost(ApiEndpoints.Projects.ArchivePosition, Handler)
            .WithSummary("Archive a position")
            .WithName("Archive position")
            .ProducesStandardErrors()
            .Produces<ProjectPositionArchived>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] Guid positionId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<ProjectPositionArchived>(
                new ArchivePosition(
                    projectId,
                    user.GetOrganization.Value,
                    positionId,
                    user.UserId
                ),
                ct
            )
        );
}
