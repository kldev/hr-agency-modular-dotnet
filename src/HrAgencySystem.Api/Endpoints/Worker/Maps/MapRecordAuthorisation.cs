using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.WorkAuthorisations.Record;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapRecordAuthorisation
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // POST /api/workers/{workerId}/work-authorisations - a permit, a residence title or a visa.
        // Sending the id of one already on file renews it; leaving it out records a new one.
        endpoints
            .MapPost(ApiEndpoints.Workers.RecordAuthorisation, Handler)
            .WithSummary("Record a worker's permission to work")
            .WithName("Record work authorisation")
            .ProducesStandardErrors()
            .Produces<WorkAuthorisationRecorded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        RecordWorkAuthorisationRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkAuthorisationRecorded>(
            request.ToCommand(
                workerId: workerId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record RecordWorkAuthorisationRequest(
        WorkAuthorisationKind Kind,
        string Country,
        string Number,
        DateOnly ValidFrom,
        DateOnly ValidUntil,
        Guid? AuthorisationId = null,
        Guid? DocumentId = null,
        string? Note = null
    )
    {
        public RecordWorkAuthorisation ToCommand(
            Guid workerId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) =>
            new(
                workerId,
                organizationId.Value,
                AuthorisationId,
                Kind,
                Country,
                Number,
                ValidFrom,
                ValidUntil,
                DocumentId,
                Note,
                modifiedBy
            );
    }
}
