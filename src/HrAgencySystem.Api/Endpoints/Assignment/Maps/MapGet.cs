using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Projections;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.Assignment.Maps;

internal static class MapGet
{
    internal static void Map(RouteGroupBuilder endpoints)
    {
        // GET /api/assignments/{assignmentId}
        endpoints
            .MapGet(ApiEndpoints.Assignments.Get, Handler)
            .WithSummary("Get an assignment")
            .WithName("Get assignment")
            .ProducesStandardErrors()
            .Produces<AssignmentProjection>();
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

        return TypedResults.Ok(assignment);
    }
}
