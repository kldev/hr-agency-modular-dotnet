using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Workers.Events;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapRegister
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/workers
        endpoints
            .MapPost(ApiEndpoints.Workers.Register, Handler)
            .WithSummary("Register a worker")
            .WithName("Register worker")
            .ProducesStandardErrors()
            .Produces<WorkerRegistered>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        WorkerRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkerRegistered>(
            request.ToRegisterCommand(organizationId: user.GetOrganization, createdBy: user.UserId),
            ct
        );

        return TypedResults.Created($"/api/workers/{result.WorkerId}", result);
    }
}
