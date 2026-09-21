using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/assignments - the register, read either way round: by person for a history, by
        // project for a crew.
        endpoints
            .MapGet(ApiEndpoints.Assignments.Slice, Handler)
            .WithSummary("Get a slice of assignments")
            .WithName("Get assignments")
            .ProducesStandardErrors()
            .Produces<SliceResponse<AssignmentProjection>>();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IAssignmentsQueryRepository repository,
        [FromQuery] string? search,
        [FromQuery] AssignmentStatus[]? status,
        [FromQuery] Guid? workerId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? positionId,
        [FromQuery] string? workCountry,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetAssignments(
            user.GetOrganization,
            new AssignmentQuery(
                search ?? "",
                status,
                workerId,
                projectId,
                positionId,
                workCountry,
                page,
                pageSize
            ),
            ct
        );

        return TypedResults.Ok(result);
    }
}
