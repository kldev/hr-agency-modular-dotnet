using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapUpdate
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/workers/{workerId}
        endpoints
            .MapPut(ApiEndpoints.Workers.Update, Handler)
            .WithSummary("Update a worker")
            .WithName("Update worker")
            .ProducesStandardErrors()
            .Produces<WorkerUpdated>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        WorkerRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkerUpdated>(
            request.ToUpdateCommand(
                workerId: workerId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
