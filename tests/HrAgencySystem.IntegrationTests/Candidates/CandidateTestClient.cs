using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Candidate.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Events.Candidates;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Web;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Candidates;

public sealed class CandidateTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/recruitment/candidates";
    internal async Task<CandidateCreated> Create(
        Guid? organizationId =null,
        string? Email = null,
        string FirstName = "joe",
        string LastName = "smith",
        Guid? createdByUserId =null,
        string? Note = null,
        string? Phone =null,
        CandidateSource source = CandidateSource.Facebook)
    {
        client.WithUserId(createdByUserId ?? Guid.NewGuid());
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());


        var command = new CreateCandidateRequest(
            Email ?? "email@fake.com",
            Phone ?? "+1 123 123 123",
            FirstName,
            LastName,
            source,
            Note ??""
            );

        var response = await client.PostAsJsonAsync(
            BaseUrl,
            command);

        var result = await response.ReadWithJson<CandidateCreated>();
        
        response.EnsureSuccessStatusCode();
        return result!;
    }

    internal async Task<CandidateUpdated> Update(
        Guid candidateId,
        Guid? organizationId = null,
        string FirstName = "joe",
        string LastName = "smith",
        Guid? modifiedByUserId = null,
        string? Note = null,
        string? Phone = null
    )
    {
        client.WithUserId(modifiedByUserId ?? Guid.NewGuid());
        client.WithOrganizationId(organizationId ?? Guid.NewGuid());


        var command = new MapUpdate.UpdateCandidateRequest(
            Phone ?? "+1 123 123 123",
            FirstName,
            LastName,
            Note ?? ""
        );

        var response = await client.PutAsJsonAsync(
            $"{BaseUrl}/{candidateId}",
            command);

        var result = await response.ReadWithJson<CandidateUpdated>();

        response.EnsureSuccessStatusCode();
        
        return result!;
    }
    
    internal async Task<CandidateProjection?> GetAsync(
        Guid organizationId,
        Guid candidateId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync(
            $"{BaseUrl}/{candidateId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<CandidateProjection>();
    }

    internal async Task<SliceResponse<CandidateProjection>> GetSliceAsync ( 
        Guid organizationId,
        int? page,
        int? pageSize)
    {
        var sliceUrl = $"{BaseUrl}";
        var query = new List<string>();
        
        query.Add($"page={page ?? 1}");
        query.Add($"pageSize={pageSize ?? 100}");
        
        client.WithOrganizationId(organizationId);
        sliceUrl += $"?{string.Join("&", query)}";

        var response = await client.GetAsync(
            $"{sliceUrl}");

        
        response.EnsureSuccessStatusCode();

        return (await response.ReadWithJson<SliceResponse<CandidateProjection>>())!;
    }
}
