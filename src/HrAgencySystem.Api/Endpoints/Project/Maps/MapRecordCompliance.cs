using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Compliance.Record;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.SharedKernel.Tenant;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapRecordCompliance
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/projects/{projectId}/compliance/{requirement} - one requirement, replaced whole.
        endpoints
            .MapPut(ApiEndpoints.Projects.RecordCompliance, Handler)
            .WithSummary("Record a compliance item")
            .WithName("Record compliance item")
            .ProducesStandardErrors()
            .Produces<ComplianceItemRecorded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid projectId,
        [FromRoute] ComplianceRequirement requirement,
        RecordComplianceItemRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ComplianceItemRecorded>(
            request.ToCommand(
                projectId: projectId,
                organizationId: user.GetOrganization,
                requirement: requirement,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record RecordComplianceItemRequest(
        [property: Description(
            "NotStarted, InProgress, Confirmed, NotApplicable or Expired. The requirement itself is in the route."
        )]
            ComplianceStatus Status,
        [property: Description(
            "The number of the licence, notification or registration, as issued."
        )]
            string? ReferenceNumber,
        [property: Description("First day the confirmation applies.")] DateOnly? ValidFrom,
        [property: Description("Last day it applies; not before ValidFrom.")] DateOnly? ValidTo,
        [property: Description(
            "Optional project document that proves it. A document recorded as proof cannot be removed."
        )]
            Guid? DocumentId,
        [property: Description("Optional note.")] string? Note
    )
    {
        public RecordComplianceItem ToCommand(
            Guid projectId,
            OrganizationId organizationId,
            ComplianceRequirement requirement,
            Guid modifiedBy
        ) =>
            new(
                projectId,
                organizationId.Value,
                requirement,
                Status,
                ReferenceNumber,
                ValidFrom,
                ValidTo,
                DocumentId,
                Note,
                modifiedBy
            );
    }
}
