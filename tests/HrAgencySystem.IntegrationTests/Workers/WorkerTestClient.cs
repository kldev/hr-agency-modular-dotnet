using System.Net.Http.Json;
using HrAgencySystem.Compliance;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Projections;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

/// <summary>
/// Builds worker and assignment fixtures through the real HTTP surface, so a test never sets up a
/// state the API could not have produced.
/// </summary>
public sealed class WorkerTestClient(HttpClient client, ITestOutputHelper output)
{
    public async Task<WorkerRegistered> RegisterAsync(
        Guid organizationId,
        string citizenship = WorkerTestData.PolishCitizenship,
        string? documentNumber = null,
        string firstName = "Jan",
        string lastName = "Kowalski"
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/workers",
            WorkerTestData.RegisterRequest(
                documentNumber: documentNumber,
                citizenship: citizenship,
                firstName: firstName,
                lastName: lastName,
                issuingCountry: citizenship
            )
        );
        response.EnsureSuccessStatusCode();

        var created = await response.ReadWithJson<WorkerRegistered>(output);
        Assert.NotNull(created);

        return created;
    }

    public async Task ChangeStatusAsync(Guid organizationId, Guid workerId, WorkerStatus status)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/workers/{workerId}/status",
            WorkerTestData.WorkerStatusRequest(status)
        );
        response.EnsureSuccessStatusCode();
    }

    /// <summary>Walks somebody all the way through the pipeline, skipping what does not apply.</summary>
    public async Task<Guid> EmployedAsync(
        Guid organizationId,
        string citizenship = WorkerTestData.PolishCitizenship,
        string? documentNumber = null
    )
    {
        var worker = await RegisterAsync(organizationId, citizenship, documentNumber);

        await ChangeStatusAsync(organizationId, worker.WorkerId, WorkerStatus.ContractPreparation);

        if (LegalisationPolicy.RequiresLegalisation(citizenship))
            await ChangeStatusAsync(organizationId, worker.WorkerId, WorkerStatus.Legalisation);

        await ChangeStatusAsync(organizationId, worker.WorkerId, WorkerStatus.Onboarding);
        await ChangeStatusAsync(organizationId, worker.WorkerId, WorkerStatus.Employed);

        return worker.WorkerId;
    }

    public async Task<AssignmentPlanned> PlanAsync(
        Guid organizationId,
        Guid workerId,
        Guid projectId,
        Guid positionId,
        EngagementType engagementType = EngagementType.PostingOfWorkers,
        DateOnly? startsOn = null,
        DateOnly? endsOn = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PostAsJsonAsync(
            "/api/assignments",
            WorkerTestData.PlanRequest(
                workerId,
                projectId,
                positionId,
                engagementType,
                startsOn,
                endsOn
            )
        );
        response.EnsureSuccessStatusCode();

        var planned = await response.ReadWithJson<AssignmentPlanned>(output);
        Assert.NotNull(planned);

        return planned;
    }

    public async Task ChangeAssignmentStatusAsync(
        Guid organizationId,
        Guid assignmentId,
        AssignmentStatus status,
        DateOnly? endsOn = null
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.PutAsJsonAsync(
            $"/api/assignments/{assignmentId}/status",
            WorkerTestData.AssignmentStatusRequest(status, endsOn)
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<WorkerProjection?> GetAsync(Guid organizationId, Guid workerId)
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/workers/{workerId}");

        return response.IsSuccessStatusCode
            ? await response.ReadWithJson<WorkerProjection>(output)
            : null;
    }

    public async Task<AssignmentProjection?> GetAssignmentAsync(
        Guid organizationId,
        Guid assignmentId
    )
    {
        client.WithOrganizationId(organizationId);

        var response = await client.GetAsync($"/api/assignments/{assignmentId}");

        return response.IsSuccessStatusCode
            ? await response.ReadWithJson<AssignmentProjection>(output)
            : null;
    }
}
