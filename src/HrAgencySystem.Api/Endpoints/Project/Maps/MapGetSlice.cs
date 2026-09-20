using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Project.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/projects
        endpoints
            .MapGet(ApiEndpoints.Projects.Slice, Handler)
            .WithSummary("Get projects")
            .WithName("Get projects")
            .ProducesStandardErrors()
            .Produces<SliceResponse<ProjectProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IProjectsQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] ProjectStatus[]? status,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? teamId,
        [FromQuery] string? country,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetProjects(
            user.GetOrganization,
            new ProjectQuery(search ?? "", status, companyId, teamId, country, page, pageSize),
            ct
        );

        return TypedResults.Ok(result);
    }
}
