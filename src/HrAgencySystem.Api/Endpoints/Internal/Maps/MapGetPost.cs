using HrAgencySystem.Api.Common.Response;
using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Api.Endpoints.Internal.Maps;

internal static class MapGetPost
{
    internal static void Map(RouteGroupBuilder group) =>
        group.MapGet(ApiEndpoints.Internal.Post, Handler).WithSummary("One published post");

    private static async Task<IResult> Handler(
        IQueryOrganizationRepository organizations,
        IJobPostQueryRepository posts,
        string slug,
        string postSlug,
        CancellationToken ct
    )
    {
        var post = await BoardPosts.FindPublishedAsync(organizations, posts, slug, postSlug, ct);

        return post is null
            ? TypedResults.NotFound(DomainObjectNotFound.NotFound("Post", $"{slug}/{postSlug}"))
            : TypedResults.Ok(
                new BoardPostResponse(
                    post.Title,
                    post.Description,
                    post.Location,
                    post.EmploymentType,
                    post.Responsibilities,
                    post.Requirements
                )
            );
    }

    internal sealed record BoardPostResponse(
        string Title,
        string Description,
        string Location,
        EmploymentType EmploymentType,
        IReadOnlyList<string> Responsibilities,
        IReadOnlyList<string> Requirements
    );
}
