using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.AssignmentCompliance.Record;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapRecordCompliance
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // PUT /api/assignments/{assignmentId}/compliance/{requirement} - recording is replacing:
        // there is one answer per requirement per posting, and it is the current one.
        endpoints
            .MapPut(ApiEndpoints.Assignments.RecordCompliance, Handler)
            .WithSummary("Record a compliance requirement for an assignment")
            .WithName("Record assignment compliance item")
            .ProducesStandardErrors()
            .Produces<AssignmentComplianceItemRecorded>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        [FromRoute] Guid assignmentId,
        [FromRoute] ComplianceRequirement requirement,
        RecordAssignmentComplianceItemRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<AssignmentComplianceItemRecorded>(
            request.ToCommand(
                assignmentId: assignmentId,
                organizationId: user.GetOrganization,
                requirement: requirement,
                modifiedBy: user.UserId
            ),
            ct
        );

        return TypedResults.Ok(result);
    }

    internal sealed record RecordAssignmentComplianceItemRequest(
        [property: Description(
            "NotStarted, InProgress, Confirmed, NotApplicable or Expired. The requirement (e.g. A1) is in the route."
        )]
            ComplianceStatus Status,
        [property: Description("The certificate's or notification's number, as issued.")]
            string? ReferenceNumber = null,
        [property: Description("First day it applies.")] DateOnly? ValidFrom = null,
        [property: Description("Last day it applies; not before ValidFrom.")]
            DateOnly? ValidTo = null,
        [property: Description("Optional assignment document that proves it.")]
            Guid? DocumentId = null,
        [property: Description("Optional note.")] string? Note = null
    )
    {
        public RecordAssignmentComplianceItem ToCommand(
            Guid assignmentId,
            OrganizationId organizationId,
            ComplianceRequirement requirement,
            Guid modifiedBy
        ) =>
            new(
                assignmentId,
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
