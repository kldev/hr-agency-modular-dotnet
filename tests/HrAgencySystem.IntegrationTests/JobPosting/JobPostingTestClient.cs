using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.JobPosting.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.IntegrationTests.JobPosting;

public sealed class JobPostingTestClient(
    HttpClient client)
{
    private const string RestUrl = "/api/recruitment/job-posting";
    internal void WithOrganizationId(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);
    }
    
    internal async Task<JobPostProjection> GetSingle(Guid jobPostId)
    {
        var response = await client.GetAsync($"{RestUrl}/{jobPostId}");
        
        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<JobPostProjection>();

        Assert.NotNull(result);
        return result;
    }
    
    internal async Task<JobPostCreated> CreateAsync(
        CreatePostRequest request)
    {
        var response = await client.PostAsJsonAsync(
            RestUrl,
            request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.ReadWithJson<JobPostCreated>();

        Assert.NotNull(result);

        return result;
    }

}