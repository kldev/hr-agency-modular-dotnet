using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Request;
using HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;
using HrAgencySystem.Recruitment.Events.JobPosting;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapChangeRecruiter
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-posting/{jobPostId:guid}/change-recruiter
        group.MapPut("/{jobPostId:guid}/change-recruiter", Handler).WithSummary("Change recruiter");
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, Guid jobPostId, AssignRecruiter request,
        IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<JobPostRecruiterChanged>(
            new ChangeJobPostRecruiter(jobPostId,  user.OrganizationId, request.RecruiterId, user.UserId), ct);

        return TypedResults.Ok(result);
    }
}