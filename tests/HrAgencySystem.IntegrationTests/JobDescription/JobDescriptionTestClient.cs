using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.JobDescription.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.JobDescription.Application.Result;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Projections;

namespace HrAgencySystem.IntegrationTests.JobDescription;

public sealed class JobDescriptionTestClient(
    HttpClient client)
{

    internal void WithOrganizationId(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);
    }
    
    internal async Task<JobDescriptionProjection> GetSingle(Guid jobDescriptionId)
    {
        var response = await client.GetAsync($"/api/job-description/{jobDescriptionId}");
        
        response.EnsureSuccessStatusCode();

        var result = await response.ReadWithJson<JobDescriptionProjection>();

        Assert.NotNull(result);
        return result;
    }
    
    internal async Task<JobDescriptionCreated> CreateAsync(
        CreateJobDescriptionRequest request)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/job-description",
            request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.ReadWithJson<JobDescriptionCreated>();

        Assert.NotNull(result);

        return result;
    }
    
    internal async Task<UpdateJobDescriptionStatusResult> ChangeStatusAsync(
        
        Guid jobDescriptionId,
        JobDescriptionStatus status)
    {
        var response = await client.PutAsync(
            $"/api/job-description/{jobDescriptionId}/{status}",
            null);

        response.EnsureSuccessStatusCode();

        var result =
            await response.ReadWithJson<UpdateJobDescriptionStatusResult>();

        Assert.NotNull(result);

        return result;
    }

    internal async Task<HttpResponseMessage> GetStatusHistoryAsync(
        Guid? jobDescriptionId = null)
    {
        var url = "/api/job-description/status";

        if (jobDescriptionId.HasValue)
        {
            url += $"?jobDescriptionId={jobDescriptionId.Value}";
        }

        return await client.GetAsync(url);
    }
}