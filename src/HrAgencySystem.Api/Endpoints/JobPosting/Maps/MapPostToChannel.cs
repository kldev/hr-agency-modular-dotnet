using System.ComponentModel;
using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapPostToChannel
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-posting/{id}/channel
        group
            .MapPut(ApiEndpoints.Recruitment.JobPosts.PostToChannel, Handler)
            .WithSummary("Job posted to channel")
            .WithName("Post job to channel")
            .Produces<JobPostedToChannel>()
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        PostToChannelRequest request,
        Guid jobPostId,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<JobPostedToChannel>(
            request.ToCommand(user.OrganizationId, user.UserId, jobPostId),
            ct
        );

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record PostToChannelRequest(
    [property: Description(
        "Where the post went: CareerPage, PracujPl, Olx, PracaPl, Rocketjobs, JustJoinIt, NoFluffJobs, Linkedin, Indeed or Other. It records the publication, it does not publish anything. A post not yet published becomes Published; a closed one is refused."
    )]
        PostingChannelType Channel
)
{
    internal PostToChannel ToCommand(Guid organizationId, Guid userId, Guid jobPostId) =>
        new PostToChannel(jobPostId, organizationId, Channel, userId);
}
