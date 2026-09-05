using HrAgencySystem.Api.Auth;
using HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;
using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Events.JobPosting;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapPostToChannel
{
    internal static void Map(RouteGroupBuilder group)
    {
        // PUT /api/recruitment/job-posting/{id}/channel 
        group.MapPut("{jobPostId:guid}/channel", Handler);
    }

    private static async Task<IResult> Handler(
        AppUserAuthenticated user,
        IMessageBus bus,
        PostToChannelRequest request,
        Guid jobPostId,
        CancellationToken ct)
    {
        var result =
            await bus.InvokeAsync<JobPostedToChannel>(
                request.ToCommand(user.OrganizationId, user.UserId, jobPostId),
                ct);

        return TypedResults.Ok(result);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed record PostToChannelRequest(PostingChannelType Channel)
{
    internal PostToChannel ToCommand(Guid organizationId, Guid userId, Guid jobPostId)
        => new PostToChannel(jobPostId, organizationId, Channel, userId);
}