using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobPosting.ChangeStatus;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;


internal static class MapChangeStatus
{
    // PUT /api/recruitment/job-posting/{jobPostId:guid}/{id}/status
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{jobPostId}/status", Handler)
            .WithSummary("Change status")
            .WithName("Change job post status")
            .Produces<JobPostStatusChanged>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(IMessageBus bus, AppUserAuthenticated user, Guid jobPostId,
        ChangeJobPostStatusRequest request)
    {
        var result =
            await bus.InvokeAsync<JobPostStatusChanged>(request.ToCommand(jobPostId,
                user.OrganizationId, user.UserId));

        // send to wolverine handler
        await bus.PublishAsync(result);
        
        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangeJobPostStatusRequest(
    JobPostStatusApi Status
)
{
    public ChangeJobPostStatus ToCommand(Guid jobPostId, Guid organizationId, Guid userId)
        => new (jobPostId, organizationId, Status, userId);
}