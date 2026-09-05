using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.Port;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

public class MapGet
{
    // GET /api/recruitment/job-applications/{id}
    internal static void Map(RouteGroupBuilder group)
    {
        // api/recruitment/candidates
        group.MapGet("{jobApplicationId:guid}", Handler).WithSummary("Gets a  Job Application")
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IJobApplicationQueryRepository repository,
        Guid jobApplicationId, CancellationToken ct)
    {
        var result = await repository.GetJobApplication(user.OrganizationId, jobApplicationId, ct);

        if (result == null)
        {
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Job Application", jobApplicationId));
        }

        return TypedResults.Ok(result);
    }
}