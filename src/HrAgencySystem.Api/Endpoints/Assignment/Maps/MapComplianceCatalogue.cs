using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapComplianceCatalogue
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/assignments/{assignmentId}/compliance/catalogue - what this person needs for
        // this posting: the A1, the declaration naming them, the local contract. The project's own
        // catalogue answers the other half, the obligations the delivering company carries.
        endpoints
            .MapGet(ApiEndpoints.Assignments.ComplianceCatalogue, Handler)
            .WithSummary("Get the compliance requirements of an assignment")
            .WithName("Get assignment compliance catalogue")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<ComplianceRequirementView>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IAssignmentsQueryRepository repository,
        [FromRoute] Guid assignmentId,
        CancellationToken ct
    )
    {
        var assignment =
            await repository.GetAssignment(user.GetOrganization, assignmentId, ct)
            ?? throw new NotFoundException("Assignment", assignmentId);

        var recorded = assignment.Compliance.ToDictionary(c => c.Requirement);

        var result = ComplianceCatalogue
            .For(assignment.WorkCountry, assignment.EngagementType, ComplianceScope.Assignment)
            .Select(requirement => new ComplianceRequirementView(
                requirement,
                ComplianceCatalogue.RequiresReferenceNumber(requirement),
                recorded.GetValueOrDefault(requirement)
            ))
            .ToList();

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// A requirement paired with whatever has been recorded against it, so an untouched requirement
    /// shows up as an empty row rather than as an absence.
    /// </summary>
    internal sealed record ComplianceRequirementView(
        ComplianceRequirement Requirement,
        bool RequiresReferenceNumber,
        ComplianceItem? Item
    );
}
