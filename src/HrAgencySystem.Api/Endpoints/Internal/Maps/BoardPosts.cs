using HrAgencySystem.Recruitment.Application.JobPosting.Queries;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Services;

namespace HrAgencySystem.Api.Endpoints.Internal.Maps;

/// <summary>
/// A post as the board may see it: found by the agency's slug and the post's own, and only when
/// published. A draft or a closed post answers 404 - the board lists published posts only, and one
/// reached by an old link should not be shown as if it still took applications.
/// </summary>
internal static class BoardPosts
{
    public static async Task<JobPostProjection?> FindPublishedAsync(
        IQueryOrganizationRepository organizations,
        IJobPostQueryRepository posts,
        string slug,
        string postSlug,
        CancellationToken ct
    )
    {
        var organization = await organizations.GetBySlugAsync(slug, ct);

        if (organization is null)
            return null;

        var post = await posts.GetJobPost(organization.Id, $"{organization.Slug}/{postSlug}", ct);

        return post is { Status: JobPostStatus.Published } ? post : null;
    }
}
