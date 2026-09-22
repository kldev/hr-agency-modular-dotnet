using System.ComponentModel;
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
        [property: Description("WorkPermit, ResidencePermit, Visa, WorkStatement or Other.")]
            WorkAuthorisationKind Kind,
        [property: Description("Country the permission is for, ISO 3166-1 alpha-2.")]
            string Country,
        [property: Description("The permission's number, as issued.")] string Number,
        [property: Description("First day it is valid.")] DateOnly ValidFrom,
        [property: Description("Last day it is valid.")] DateOnly ValidUntil,
        [property: Description(
            "Omit to add a permission; give the id of one on the file to replace it."
        )]
            Guid? AuthorisationId = null,
        [property: Description("Optional document on the person's file that proves it.")]
            Guid? DocumentId = null,
        [property: Description("Optional note.")] string? Note = null
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
