using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Tasks.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Tasks.Application.Port;
using HrAgencySystem.Tasks.Domain;
using HrAgencySystem.Tasks.Events;
using HrAgencySystem.Tasks.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Tasks;

/// <summary>Tasks through the real HTTP surface, always as a named person of a named organization.</summary>
public sealed class TasksTestClient(HttpClient client, ITestOutputHelper output)
{
    public const string BaseUrl = "/api/tasks";

    public const string TimeZone = "Europe/Warsaw";

    internal static CreateTaskRequest Request(
        Guid companyId,
        DateTimeOffset dueAt,
        string title = "Call HR Manager",
        Guid? opportunityId = null,
        Guid? assigneeId = null,
        TaskPriority priority = TaskPriority.Medium
    ) => new(companyId, opportunityId, title, null, dueAt, priority, assigneeId);

    public async Task<HttpResponseMessage> CreateResponseAsync(
        Guid organizationId,
        Guid userId,
        object request
    )
    {
        As(organizationId, userId);

        return await client.PostAsJsonAsync(BaseUrl, request);
    }

    internal async Task<TaskItemCreated> CreateAsync(
        Guid organizationId,
        Guid userId,
        CreateTaskRequest request
    )
    {
        var response = await CreateResponseAsync(organizationId, userId, request);
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<TaskItemCreated>(output);
        Assert.NotNull(created);

        return created;
    }

    public async Task<TaskBoard> BoardAsync(
        Guid organizationId,
        Guid userId,
        TaskRangeKind range,
        Guid? companyId = null
    )
    {
        As(organizationId, userId);

        var url = $"{BaseUrl}?range={range}&timeZone={Uri.EscapeDataString(TimeZone)}";
        if (companyId is not null)
            url += $"&companyId={companyId}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var board = await response.ReadWithJson<TaskBoard>();
        Assert.NotNull(board);

        return board;
    }

    public async Task<HttpResponseMessage> GetResponseAsync(Guid organizationId, Guid userId, Guid taskId)
    {
        As(organizationId, userId);

        return await client.GetAsync($"{BaseUrl}/{taskId}");
    }

    public async Task<TaskItemProjection?> GetAsync(Guid organizationId, Guid userId, Guid taskId)
    {
        var response = await GetResponseAsync(organizationId, userId, taskId);

        return response.IsSuccessStatusCode ? await response.ReadWithJson<TaskItemProjection>() : null;
    }

    public async Task<HttpResponseMessage> CompleteResponseAsync(Guid organizationId, Guid userId, Guid taskId)
    {
        As(organizationId, userId);

        return await client.PostAsync($"{BaseUrl}/{taskId}/complete", null);
    }

    public async Task CompleteAsync(Guid organizationId, Guid userId, Guid taskId) =>
        (await CompleteResponseAsync(organizationId, userId, taskId)).EnsureSuccessStatusCode();

    public async Task ReopenAsync(Guid organizationId, Guid userId, Guid taskId)
    {
        As(organizationId, userId);

        (await client.PostAsync($"{BaseUrl}/{taskId}/reopen", null)).EnsureSuccessStatusCode();
    }

    internal async Task<HttpResponseMessage> UpdateResponseAsync(
        Guid organizationId,
        Guid userId,
        Guid taskId,
        UpdateTaskRequest request
    )
    {
        As(organizationId, userId);

        return await client.PutAsJsonAsync($"{BaseUrl}/{taskId}", request);
    }

    private void As(Guid organizationId, Guid userId)
    {
        client.WithOrganizationId(organizationId);
        client.WithUserId(userId);
    }
}
