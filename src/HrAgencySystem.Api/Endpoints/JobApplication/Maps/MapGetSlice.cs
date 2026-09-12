using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Api.Endpoints.JobApplication.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        // GET /api/recruitment/job-applications
        group.MapGet("", Handler)
            .WithSummary("Get applications")
            .WithName("Get job applications slice")
            .Produces<SliceResponse<JobApplicationProjection>>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user,
        IJobApplicationQueryRepository repository,
        string? search, 
        Guid? companyId, 
        Guid[]? tag,
        JobApplicationStatus[]? status,
        CandidateSource[]? source,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new JobApplicationQuery(search ?? "", companyId, tag ?? [], status, source ?? [], page, pageSize);
        var result = await repository.GetJobApplications(user.OrganizationId, query, ct);
        
        return TypedResults.Ok(result);
    }
}