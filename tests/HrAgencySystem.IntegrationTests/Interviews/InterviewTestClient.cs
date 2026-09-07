using HrAgencySystem.Api.Endpoints.Interviews.Maps;
using HrAgencySystem.Recruitment.Events.Interviews;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Interviews;

using System.Net.Http.Json;
using Infrastructure;
using HrAgencySystem.Recruitment.Domain.Interviews;
using Recruitment.Projections;
using SharedKernel.Web;

public sealed class InterviewTestClient(HttpClient client, ITestOutputHelper output)
{
    public async Task<InterviewCreated> ScheduleAsync(
        Guid organizationId,
        Guid jobApplicationId,
        DateTime scheduledAt,
        Guid interviewerId,
        Guid createdByUserId,
        string scheduledTimezone = "Europe/Warsaw")
    {
        client.WithUserId(createdByUserId);
        client.WithOrganizationId(organizationId);


        var command = new ScheduleInterviewRequest(
            jobApplicationId,
            scheduledAt,
            InterviewFormat.Online,
            InterviewType.Technical,
            "",
            interviewerId,
            scheduledTimezone);

        var response = await client.PostAsJsonAsync(
            "/api/interviews/schedule",
            command);

        var result = await response.ReadWithJson<InterviewCreated>();
        
        response.EnsureSuccessStatusCode();
        return result!;
    }

    public async Task<InterviewProjection?> GetAsync(
        Guid organizationId,
        Guid interviewId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync(
            $"/api/interviews/{interviewId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.ReadWithJson<InterviewProjection>();
    }

    public async Task<SliceResponse<InterviewProjection>> GetSliceAsync(
        Guid organizationId,
        Guid? jobApplicationId = null,
        Guid? interviewerId = null,
        Guid? candidateId = null,
        Guid? createdByUserId = null,
        InterviewStatus? status = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string timezone = "Europe/Warsaw",
        int page = 1,
        int pageSize = 100)
    {
        client.WithOrganizationId(organizationId);

        var query = new List<string>();

        if (jobApplicationId.HasValue)
            query.Add($"jobApplicationId={jobApplicationId}");

        if (interviewerId.HasValue)
            query.Add($"interviewerId={interviewerId}");

        if (candidateId.HasValue)
            query.Add($"candidateId={candidateId}");

        if (createdByUserId.HasValue)
            query.Add($"createdByUserId={createdByUserId}");

        if (status.HasValue)
            query.Add($"status={status}");

        if (fromDate.HasValue)
            query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

        if (toDate.HasValue)
            query.Add($"toDate={toDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(timezone))
            query.Add($"timezone={Uri.EscapeDataString(timezone)}");

        query.Add($"page={page}");
        query.Add($"pageSize={pageSize}");

        var url = query.Count == 0
            ? "/api/interviews"
            : $"/api/interviews?{string.Join("&", query)}";

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var result =
            await response.ReadWithJson<SliceResponse<InterviewProjection>>(output);

        Assert.NotNull(result);

        return result;
    }
}
