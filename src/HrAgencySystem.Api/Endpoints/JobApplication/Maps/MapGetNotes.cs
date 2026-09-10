using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Errors;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using Microsoft.AspNetCore.Mvc;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;


internal static class MapGetNotes
{
    
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications/{id}/notes
        group.MapGet("{jobApplicationId:guid}/notes", Handler).WithSummary("Get notes")
            .Produces<IReadOnlyList<ApplicationNoteItem>>()
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<BadRequestDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        INoteQueryRepository repository,
        Guid jobApplicationId, CancellationToken ct)
    {
        var result = await repository.GetNotes(user.OrganizationId, jobApplicationId, ct);

        return TypedResults.Ok(result);
    }
}