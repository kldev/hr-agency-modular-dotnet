using HrAgencySystem.Api.Auth;
using HrAgencySystem.Api.Common.Config;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.SharedKernel.Web;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Endpoints.JobPosting.Maps;

internal static class MapGetSlice
{
    internal static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/api/recruitment/job-posting", Handler).WithSummary("Get job posts");
    }

    private static async Task<IResult> Handler(IJobPostQueryRepository repository, IOptions<ApplicationConfig> config,
        AppUserAuthenticated user,
        string? search,
        Guid? companyId,
        Guid? recruiterId,
        JobPostStatus[]? status,
        string[]? lang,
        int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var appUrl = config.Value.AppUrl;

        var query = new JobPostQuery(search ?? "", companyId, recruiterId, status ?? [], lang ?? [], page, pageSize);
        var result = await repository.GetJobPosts(user.OrganizationId, query, ct);

        var content = result.Content.Select(z => z with { JobUrl = $"{appUrl}/{z.JobUrl}" }).ToList();

        return Results.Ok(result with { Content = content });

    }
}