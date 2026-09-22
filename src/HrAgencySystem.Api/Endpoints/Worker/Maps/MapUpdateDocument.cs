using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.WorkerDocuments.UpdateMetadata;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Worker.Maps;

internal static class MapUpdateDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/workers/{workerId}/documents/{documentId} - the description only. Replacing the
        // file itself means attaching a new document.
        endpoints
            .MapPut(ApiEndpoints.Workers.UpdateDocument, Handler)
            .WithSummary("Update a worker document's details")
            .WithName("Update worker document")
            .ProducesStandardErrors()
            .Produces<WorkerDocumentMetadataChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid workerId,
        [FromRoute] Guid documentId,
        UpdateWorkerDocumentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<WorkerDocumentMetadataChanged>(
            request.ToCommand(
                workerId: workerId,
                documentId: documentId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record UpdateWorkerDocumentRequest(
        [property: Description(
            "Identity, EmploymentContract, MedicalCertificate, HealthAndSafety, Qualification, Legalisation or Other."
        )]
            WorkerDocumentCategory Category,
        [property: Description("The date on the document.")] DateOnly DocumentDate,
        [property: Description(
            "Last day it is valid, when it expires - e.g. a medical certificate."
        )]
            DateOnly? ValidUntil = null,
        [property: Description("Optional note.")] string? Note = null
    )
    {
        public UpdateWorkerDocumentMetadata ToCommand(
            Guid workerId,
            Guid documentId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) =>
            new(
                workerId,
                organizationId.Value,
                documentId,
                Category,
                DocumentDate,
                ValidUntil,
                Note,
                modifiedBy
            );
    }
}
