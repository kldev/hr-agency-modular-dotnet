using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.AssignmentDocuments.UpdateMetadata;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapUpdateDocument
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/assignments/{assignmentId}/documents/{documentId}
        endpoints
            .MapPut(ApiEndpoints.Assignments.UpdateDocument, Handler)
            .WithSummary("Update an assignment document's details")
            .WithName("Update assignment document")
            .ProducesStandardErrors()
            .Produces<AssignmentDocumentMetadataChanged>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        [FromRoute] Guid documentId,
        UpdateAssignmentDocumentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentDocumentMetadataChanged>(
            request.ToCommand(
                assignmentId: assignmentId,
                documentId: documentId,
                organizationId: user.GetOrganization,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record UpdateAssignmentDocumentRequest(
        [property: Description(
            "Contract, SocialSecurity, HostCountryNotification, Compliance or Other."
        )]
            AssignmentDocumentCategory Category,
        [property: Description("The date on the document.")] DateOnly DocumentDate,
        [property: Description("Last day it is valid, when it expires.")]
            DateOnly? ValidUntil = null,
        [property: Description("Optional note.")] string? Note = null
    )
    {
        public UpdateAssignmentDocumentMetadata ToCommand(
            Guid assignmentId,
            Guid documentId,
            OrganizationId organizationId,
            Guid modifiedBy
        ) =>
            new(
                assignmentId,
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
