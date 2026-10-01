using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Interviews.Maps;
using HrAgencySystem.Api.Endpoints.JobApplication.Maps;
using HrAgencySystem.Api.Endpoints.JobPosting.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.JobPosts;
using HrAgencySystem.Recruitment.Application.Timeline.Queries;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Interviews;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Events.Interviews;
using HrAgencySystem.Recruitment.Events.JobPostings;

namespace HrAgencySystem.IntegrationTests.Timeline;

/// <summary>
/// Builds a candidate's history through the same endpoints the panel uses, then reads it back.
/// </summary>
public sealed class TimelineTestClient(HttpClient client)
{
    private const string JobPostUrl = "/api/recruitment/job-posting";
    private const string ApplicationUrl = "/api/recruitment/job-applications";
    private const string CandidateUrl = "/api/recruitment/candidates";

    internal async Task<Guid> PublishedJobPostAsync(Guid organizationId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(JobPostUrl, JobPostingTestData.CreateRequest());
        response.EnsureSuccessStatusCode();
        var created = await response.ReadWithJson<JobPostCreated>();
        Assert.NotNull(created);

        var published = await client.PutAsJsonAsync(
            $"{JobPostUrl}/{created.JobPostId}/status",
            new ChangeJobPostStatusRequest(JobPostStatusApi.Published)
        );
        published.EnsureSuccessStatusCode();

        return created.JobPostId;
    }

    /// <summary>
    /// Retried, because applying reads the post from its projection and the post was published
    /// a moment ago. A refused application writes nothing, so trying again is safe.
    /// </summary>
    internal async Task<JobApplicationCreated> ApplyAsync(
        Guid organizationId,
        Guid jobPostId,
        string email
    )
    {
        client.WithOrganizationId(organizationId);

        JobApplicationCreated? created = null;
        await Eventually.AssertAsync(async () =>
        {
            var response = await client.PostAsJsonAsync(
                $"{JobPostUrl}/{jobPostId}/apply",
                new ApplyToPostRequest(email, "+48600100200", FirstName: "Anna", LastName: "Nowak")
            );
            response.EnsureSuccessStatusCode();

            created = await response.ReadWithJson<JobApplicationCreated>();
        });

        Assert.NotNull(created);
        return created;
    }

    internal async Task ChangeStatusAsync(
        Guid organizationId,
        Guid jobApplicationId,
        JobApplicationUpdateStatus status,
        string? note = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"{ApplicationUrl}/{jobApplicationId}/status",
            new ChangeJobApplicationStatusRequest(status, note, null)
        );
        response.EnsureSuccessStatusCode();
    }

    internal async Task AddNoteAsync(Guid organizationId, Guid jobApplicationId, string note)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            $"{ApplicationUrl}/{jobApplicationId}/note",
            new CreateNoteRequest(note)
        );
        response.EnsureSuccessStatusCode();
    }

    internal async Task<InterviewCreated> ScheduleInterviewAsync(
        Guid organizationId,
        Guid jobApplicationId,
        DateTime scheduledAt
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/interviews/schedule",
            new ScheduleInterviewRequest(
                jobApplicationId,
                scheduledAt,
                InterviewFormat.Online,
                InterviewType.Technical,
                "",
                Guid.NewGuid()
            )
        );
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<InterviewCreated>();
        Assert.NotNull(created);
        return created;
    }

    internal async Task ChangeInterviewStatusAsync(
        Guid organizationId,
        Guid interviewId,
        InterviewStatus status
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/interviews/{interviewId}/status",
            new ChangeInterviewStatusRequest(status, null)
        );
        response.EnsureSuccessStatusCode();
    }

    internal Task<TimelinePage> GetCandidateTimelineAsync(
        Guid organizationId,
        Guid candidateId,
        string? after = null,
        int pageSize = 50
    ) => GetAsync(organizationId, $"{CandidateUrl}/{candidateId}/timeline", after, pageSize);

    internal Task<TimelinePage> GetApplicationTimelineAsync(
        Guid organizationId,
        Guid jobApplicationId,
        string? after = null,
        int pageSize = 50
    ) => GetAsync(organizationId, $"{ApplicationUrl}/{jobApplicationId}/timeline", after, pageSize);

    internal async Task<HttpStatusCode> GetCandidateTimelineStatusAsync(
        Guid organizationId,
        Guid candidateId,
        string after
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync(
            $"{CandidateUrl}/{candidateId}/timeline?after={Uri.EscapeDataString(after)}"
        );
        return response.StatusCode;
    }

    private async Task<TimelinePage> GetAsync(
        Guid organizationId,
        string url,
        string? after,
        int pageSize
    )
    {
        client.WithOrganizationId(organizationId);

        var query = $"?pageSize={pageSize}";
        if (after is not null)
            query += $"&after={Uri.EscapeDataString(after)}";

        var response = await client.GetAsync(url + query);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, raw);

        var slice = await response.ReadWithJson<TimelineSlice>();
        Assert.NotNull(slice);

        return new TimelinePage(slice, raw);
    }
}

/// <summary>The parsed page and the body as it went over the wire, to prove what is not in it.</summary>
public sealed record TimelinePage(TimelineSlice Slice, string Raw);
