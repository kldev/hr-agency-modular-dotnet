using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Forms.Application.SystemFields.Archive;
using HrAgencySystem.Forms.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.SystemFields.Maps;

internal static class MapArchive
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        endpoints
            .MapPost(ApiEndpoints.SystemFields.Archive, Handler)
            .RequireAuthorization(FormsDesignPolicy.Name)
            .WithSummary("Archive a system field")
            .WithName("Archive system field")
            .ProducesStandardErrors()
            .Produces<SystemFieldArchived>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid systemFieldId,
        IMessageBus bus,
        CancellationToken ct
    ) =>
        TypedResults.Ok(
            await bus.InvokeAsync<SystemFieldArchived>(
                new ArchiveSystemField(user.OrganizationId, systemFieldId, user.UserId),
                ct
            )
        );
}
