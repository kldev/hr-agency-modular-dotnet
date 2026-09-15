using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common;
using HrAgencySystem.Recruitment.Application.Suggestion;
using HrAgencySystem.Recruitment.Domain.JobPostings;

namespace HrAgencySystem.Api.Endpoints.Suggestion.Maps;

internal static class MapJobPosts
{
    internal static void Map(this RouteGroupBuilder group)
    {
        group.MapGet("/api/suggestion/job-posts", Handler)
            .Produces<IReadOnlyList<JobPostSuggestion>>()
            .WithName("Get job post suggestions")
            .WithSummary("Get top 25 job posts")
            .ProducesStandardErrors();
    }

    private static async Task<IResult> Handler(AppUserAuthenticated user, 
        IJobPostSuggestionRepository repository,
        CancellationToken ct,
        string? search, 
        JobPostStatus? status)
    {
        var result = 
            await repository.GetPostSuggestions(user.OrganizationId, search ?? "",status, ct);
        return TypedResults.Ok(result);
    }
}