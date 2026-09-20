using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Workers.Application.WorkAuthorisations.Remove;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapRemoveAuthorisation
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // DELETE /api/workers/{workerId}/work-authorisations/{authorisationId}
        endpoints
            .MapDelete(ApiEndpoints.Workers.RemoveAuthorisation, Handler)
            .WithSummary("Remove a worker's permission to work")
            .WithName("Remove work authorisation")
            .ProducesStandardErrors()
            .Produces<WorkAuthorisationRemoved>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        [FromRoute] Guid authorisationId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkAuthorisationRemoved>(
            new RemoveWorkAuthorisation(
                workerId,
                user.GetOrganization.Value,
                authorisationId,
                user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
