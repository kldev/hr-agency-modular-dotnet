using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;


internal static class MapGetNotes
{
    
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications/{id}/notes
        group.MapGet("{jobApplicationId:guid}/notes", Handler).WithSummary("Get notes")
            .Produces<IReadOnlyList<ApplicationNoteItem>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        INoteQueryRepository repository,
        Guid jobApplicationId, CancellationToken ct)
    {
        var result = await repository.GetNotes(user.OrganizationId, jobApplicationId, ct);

        return TypedResults.Ok(result);
    }
}