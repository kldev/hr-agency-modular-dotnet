using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/projects/{projectId} - the whole details page in one call. That is what the
        // single read model is for; assembling the screen from six requests would waste it.
        endpoints
            .MapGet(ApiEndpoints.Projects.Get, Handler)
            .WithSummary("Get project")
            .WithName("Get project")
            .ProducesStandardErrors()
            .Produces<ProjectProjection>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IProjectsQueryRepository repository,
        [FromRoute] Guid projectId,
        CancellationToken ct
    )
    {
        var result =
            await repository.GetProject(user.GetOrganization, projectId, ct)
            ?? throw new NotFoundException("Project", projectId);

        return TypedResults.Ok(result);
    }
}
