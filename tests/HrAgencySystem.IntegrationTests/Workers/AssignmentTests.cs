using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Compliance;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Projects;
using HrAgencySystem.Workers.Application.PlanAssignment;
using HrAgencySystem.Workers.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

[Collection(IntegrationCollection.Name)]
public class AssignmentTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanWorkers();
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
        await Cleaner.CleanLegalEntities();
    }

    [Fact]
    public async Task An_assignment_records_who_posted_whom_and_where()
    {
        var organizationId = Guid.NewGuid();
        var workerId = await WorkerClient.EmployedAsync(organizationId);
        var project = await NewProjectAsync(organizationId);

        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            project.ProjectId,
            project.PositionId
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await WorkerClient.GetAssignmentAsync(
                organizationId,
                planned.AssignmentId
            );

            Assert.NotNull(projection);
            Assert.Equal(workerId, projection.WorkerId);
            Assert.Equal("Jan Kowalski", projection.WorkerFullName);
            Assert.Equal("BE", projection.WorkCountry);
            Assert.Equal(AssignmentStatus.Planned, projection.Status);

            // The posting carries its own obligations, counted at its own level.
            Assert.True(projection.ComplianceRequiredCount > 0);
            Assert.Equal(projection.ComplianceRequiredCount, projection.ComplianceOutstandingCount);
        });
    }

    /// <summary>
    /// The case the whole module exists for: the same person moves on, and the record of where
    /// they were stays exactly as it was.
    /// </summary>
    [Fact]
    public async Task Moving_somebody_to_another_project_leaves_the_first_posting_intact()
    {
        var organizationId = Guid.NewGuid();
        var workerId = await WorkerClient.EmployedAsync(organizationId);

        var first = await NewProjectAsync(organizationId);
        var second = await NewProjectAsync(organizationId);

        var firstPosting = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            first.ProjectId,
            first.PositionId,
            startsOn: new DateOnly(2026, 10, 1),
            endsOn: new DateOnly(2026, 12, 31)
        );
        await WorkerClient.ChangeAssignmentStatusAsync(
            organizationId,
            firstPosting.AssignmentId,
            AssignmentStatus.Active
        );
        await WorkerClient.ChangeAssignmentStatusAsync(
            organizationId,
            firstPosting.AssignmentId,
            AssignmentStatus.Completed,
            new DateOnly(2026, 12, 31)
        );

        var secondPosting = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            second.ProjectId,
            second.PositionId,
            startsOn: new DateOnly(2027, 1, 1)
        );

        await Eventually.AssertAsync(async () =>
        {
            var completed = await WorkerClient.GetAssignmentAsync(
                organizationId,
                firstPosting.AssignmentId
            );

            Assert.NotNull(completed);
            Assert.Equal(first.ProjectId, completed.ProjectId);
            Assert.Equal(AssignmentStatus.Completed, completed.Status);
            Assert.Equal(new DateOnly(2026, 12, 31), completed.EndsOn);

            // And the person's own file now reads as a history rather than as one overwritten row.
            var worker = await WorkerClient.GetAsync(organizationId, workerId);
            Assert.NotNull(worker);
            Assert.Equal(2, worker.AssignmentCount);
            Assert.Equal(secondPosting.AssignmentId, worker.CurrentAssignmentId);
        });
    }

    [Fact]
    public async Task Nobody_holds_two_positions_over_the_same_days()
    {
        var organizationId = Guid.NewGuid();
        var workerId = await WorkerClient.EmployedAsync(organizationId);
        var first = await NewProjectAsync(organizationId);
        var second = await NewProjectAsync(organizationId);

        await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            first.ProjectId,
            first.PositionId,
            startsOn: new DateOnly(2026, 10, 1),
            endsOn: new DateOnly(2026, 12, 31)
        );

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);

            var response = await Client.PostAsJsonAsync(
                "/api/assignments",
                WorkerTestData.PlanRequest(
                    workerId,
                    second.ProjectId,
                    second.PositionId,
                    startsOn: new DateOnly(2026, 12, 1),
                    endsOn: new DateOnly(2027, 3, 31)
                )
            );

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
            Assert.Equal(PlanAssignmentHandler.OverlapsAnotherAssignmentMessage, problem?.Detail);
        });
    }

    [Fact]
    public async Task The_days_free_up_once_a_posting_has_finished()
    {
        var organizationId = Guid.NewGuid();
        var workerId = await WorkerClient.EmployedAsync(organizationId);
        var first = await NewProjectAsync(organizationId);
        var second = await NewProjectAsync(organizationId);

        var posting = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            first.ProjectId,
            first.PositionId,
            startsOn: new DateOnly(2026, 10, 1),
            endsOn: new DateOnly(2026, 12, 31)
        );
        await WorkerClient.ChangeAssignmentStatusAsync(
            organizationId,
            posting.AssignmentId,
            AssignmentStatus.DidNotStart
        );

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);

            var response = await Client.PostAsJsonAsync(
                "/api/assignments",
                WorkerTestData.PlanRequest(
                    workerId,
                    second.ProjectId,
                    second.PositionId,
                    startsOn: new DateOnly(2026, 10, 1),
                    endsOn: new DateOnly(2026, 12, 31)
                )
            );

            response.EnsureSuccessStatusCode();
        });
    }

    [Fact]
    public async Task Starting_somebody_whose_paperwork_is_unfinished_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(
            organizationId,
            WorkerTestData.UkrainianCitizenship
        );
        await WorkerClient.ChangeStatusAsync(
            organizationId,
            worker.WorkerId,
            WorkerStatus.ContractPreparation
        );
        await WorkerClient.ChangeStatusAsync(
            organizationId,
            worker.WorkerId,
            WorkerStatus.Legalisation
        );

        var project = await NewProjectAsync(organizationId);

        // Planning them is fine - that is how a crew gets scheduled while legalisation works.
        var planned = await WorkerClient.PlanAsync(
            organizationId,
            worker.WorkerId,
            project.ProjectId,
            project.PositionId
        );

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/assignments/{planned.AssignmentId}/status",
            WorkerTestData.AssignmentStatusRequest(AssignmentStatus.Active)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task An_assignment_in_another_organization_answers_404()
    {
        var organizationId = Guid.NewGuid();
        var workerId = await WorkerClient.EmployedAsync(organizationId);
        var project = await NewProjectAsync(organizationId);
        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            project.ProjectId,
            project.PositionId
        );

        Client.WithOrganizationId(Guid.NewGuid());
        var response = await Client.GetAsync($"/api/assignments/{planned.AssignmentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<ProjectTestClient.SeededDelivery> NewProjectAsync(Guid organizationId) =>
        ProjectClient.CreateWithPositionAsync(organizationId);
}
