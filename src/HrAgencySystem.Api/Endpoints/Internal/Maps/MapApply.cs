using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.JobApplications.Create;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.SharedKernel.Services;
using Wolverine;

namespace HrAgencySystem.Api.Endpoints.Internal.Maps;

/// <summary>
/// A candidate applying on the board. The same command the panel sends, so the outbox, the
/// candidate's e-mail reservation and the recruiter's mail all happen here, in one transaction,
/// instead of on a second bus in another process.
/// </summary>
internal static class MapApply
{
    internal static void Map(RouteGroupBuilder group) =>
        group
            .MapPost(ApiEndpoints.Internal.Applications, Handler)
            .WithSummary("Apply to a published post");

    private static async Task<IResult> Handler(
        IQueryOrganizationRepository organizations,
        IJobPostQueryRepository posts,
        IMessageBus bus,
        string slug,
        string postSlug,
        BoardApplicationRequest request,
        CancellationToken ct
    )
    {
        var post = await BoardPosts.FindPublishedAsync(organizations, posts, slug, postSlug, ct);

        if (post is null)
            return TypedResults.NotFound(DomainObjectNotFound.NotFound("Post", $"{slug}/{postSlug}"));

        var created = await bus.InvokeAsync<JobApplicationCreated>(
            new ApplyToJobApplication(
                post.Id,
                request.Email,
                request.Phone ?? "",
                CandidateSource.CareerPage,
                request.FirstName,
                request.LastName
            ),
            ct
        );

        return TypedResults.Created(
            (string?)null,
            new BoardApplicationResponse(created.JobApplicationId)
        );
    }

    internal sealed record BoardApplicationRequest(
        string FirstName,
        string LastName,
        string Email,
        string? Phone
    );

    internal sealed record BoardApplicationResponse(Guid ApplicationId);
}
