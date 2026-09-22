using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.ChangeWorkerStatus;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapChangeStatus
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/workers/{workerId}/status - one step along the pipeline, which also means
        // handing the file to the next department.
        endpoints
            .MapPut(ApiEndpoints.Workers.ChangeStatus, Handler)
            .WithSummary("Change a worker's status")
            .WithName("Change worker status")
            .ProducesStandardErrors()
            .Produces<WorkerStatusChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        ChangeWorkerStatusRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkerStatusChanged>(
            request.ToCommand(
                workerId: workerId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record ChangeWorkerStatusRequest(
        [property: Description(
            "Recruitment, ContractPreparation, Legalisation, Onboarding, Employed, ProjectChange or Terminated. Legalisation only exists for somebody who needs it; Terminated is reachable from anywhere."
        )]
            WorkerStatus Status,
        [property: Description("Optional note on the move, up to 500 characters.")]
            string? Reason = null
    )
    {
        public ChangeWorkerStatus ToCommand(
            Guid workerId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) => new(workerId, organizationId.Value, Status, Reason, modifiedBy);
    }
}
