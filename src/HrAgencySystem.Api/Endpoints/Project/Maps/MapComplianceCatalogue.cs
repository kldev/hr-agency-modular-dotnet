using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Compliance;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapComplianceCatalogue
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/projects/{projectId}/compliance/catalogue - what this project has to account for,
        // so nobody has to know the list in advance to notice that something is missing. Only the
        // obligations the engagement carries as a whole; what each posted person needs is on their
        // assignment, because that is the level those documents are issued at.
        endpoints
            .MapGet(ApiEndpoints.Projects.ComplianceCatalogue, Handler)
            .WithSummary("Get the compliance requirements of a project")
            .WithName("Get project compliance catalogue")
            .ProducesStandardErrors()
            .Produces<IReadOnlyList<ComplianceRequirementView>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IProjectsQueryRepository repository,
        [FromRoute] Guid projectId,
        CancellationToken ct
    )
    {
        var project =
            await repository.GetProject(user.GetOrganization, projectId, ct)
            ?? throw new NotFoundException("Project", projectId);

        var recorded = project.Compliance.ToDictionary(c => c.Requirement);

        var result = ComplianceCatalogue
            .For(project.WorkCountry, project.EngagementType, ComplianceScope.Project)
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
