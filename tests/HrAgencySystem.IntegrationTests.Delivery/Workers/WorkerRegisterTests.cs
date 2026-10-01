using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

[Collection(IntegrationCollection.Name)]
public class WorkerRegisterTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanWorkers();
    }

    [Fact]
    public async Task A_registered_worker_shows_up_in_the_register()
    {
        var organizationId = Guid.NewGuid();

        var worker = await WorkerClient.RegisterAsync(organizationId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await WorkerClient.GetAsync(organizationId, worker.WorkerId);

            Assert.NotNull(projection);
            Assert.Equal("Jan Kowalski", projection.FullName);
            Assert.Equal(WorkerStatus.Recruitment, projection.Status);
            Assert.Equal(ResponsibleDepartment.Recruitment, projection.Department);
            Assert.False(projection.RequiresLegalisation);
            Assert.Null(projection.CurrentWorkCountry);
        });
    }

    [Fact]
    public async Task A_third_country_national_is_flagged_for_legalisation()
    {
        var organizationId = Guid.NewGuid();

        var worker = await WorkerClient.RegisterAsync(
            organizationId,
            WorkerTestData.UkrainianCitizenship
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await WorkerClient.GetAsync(organizationId, worker.WorkerId);

            Assert.NotNull(projection);
            Assert.True(projection.RequiresLegalisation);
        });
    }

    [Fact]
    public async Task The_same_identity_document_cannot_open_a_second_file()
    {
        var organizationId = Guid.NewGuid();
        var documentNumber = WorkerTestData.NewDocumentNumber();

        await WorkerClient.RegisterAsync(organizationId, documentNumber: documentNumber);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PostAsJsonAsync(
            "/api/workers",
            WorkerTestData.RegisterRequest(documentNumber: documentNumber, lastName: "Kowalsky")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Equal(
            IWorkerIdentityDocumentReservationRepository.AlreadyUsedMessage,
            problem?.Detail
        );
    }

    [Fact]
    public async Task The_same_document_in_another_organization_is_a_different_person()
    {
        // Uniqueness is per tenant, like every other reservation here.
        var documentNumber = WorkerTestData.NewDocumentNumber();

        await WorkerClient.RegisterAsync(Guid.NewGuid(), documentNumber: documentNumber);
        var second = await WorkerClient.RegisterAsync(
            Guid.NewGuid(),
            documentNumber: documentNumber
        );

        Assert.NotEqual(Guid.Empty, second.WorkerId);
    }

    /// <summary>
    /// One person, one file. A second file splits somebody's postings in half, and then neither
    /// half can be asked whether they hold a valid A1 - which is the entire job of this register.
    /// </summary>
    [Fact]
    public async Task The_same_person_cannot_be_entered_twice()
    {
        var organizationId = Guid.NewGuid();
        await WorkerClient.RegisterAsync(organizationId);

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);

            // A different passport, the same human being: same name, same phone, same address.
            var response = await Client.PostAsJsonAsync(
                "/api/workers",
                WorkerTestData.RegisterRequest()
            );

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
            Assert.Contains("already on file", problem?.Detail ?? "");
        });
    }

    [Fact]
    public async Task Somebody_with_the_same_name_but_their_own_number_is_a_different_person()
    {
        var organizationId = Guid.NewGuid();
        await WorkerClient.RegisterAsync(organizationId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PostAsJsonAsync(
            "/api/workers",
            WorkerTestData.RegisterRequest() with
            {
                Email = "jan.kowalski.2@example.com",
                PhoneNumber = "+48 601 111 111",
            }
        );

        // Two people really are called Jan Kowalski; a name on its own proves nothing.
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task A_worker_from_another_organization_answers_404_rather_than_403()
    {
        // 403 would confirm that the id exists in somebody else's tenant.
        var worker = await WorkerClient.RegisterAsync(Guid.NewGuid());

        Client.WithOrganizationId(Guid.NewGuid());
        var response = await Client.GetAsync($"/api/workers/{worker.WorkerId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Changing_a_worker_in_another_organization_is_forbidden()
    {
        var worker = await WorkerClient.RegisterAsync(Guid.NewGuid());

        Client.WithOrganizationId(Guid.NewGuid());
        var response = await Client.PutAsJsonAsync(
            $"/api/workers/{worker.WorkerId}/status",
            WorkerTestData.WorkerStatusRequest(WorkerStatus.ContractPreparation)
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task The_pipeline_hands_a_polish_worker_between_departments()
    {
        var organizationId = Guid.NewGuid();
        var worker = await WorkerClient.RegisterAsync(organizationId);

        await WorkerClient.ChangeStatusAsync(
            organizationId,
            worker.WorkerId,
            WorkerStatus.ContractPreparation
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await WorkerClient.GetAsync(organizationId, worker.WorkerId);
            Assert.Equal(ResponsibleDepartment.HumanResources, projection?.Department);
        });

        // Straight past legalisation: nothing there to do for somebody with free movement.
        await WorkerClient.ChangeStatusAsync(
            organizationId,
            worker.WorkerId,
            WorkerStatus.Onboarding
        );
        await WorkerClient.ChangeStatusAsync(
            organizationId,
            worker.WorkerId,
            WorkerStatus.Employed
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await WorkerClient.GetAsync(organizationId, worker.WorkerId);
            Assert.Equal(WorkerStatus.Employed, projection?.Status);
            Assert.Equal(ResponsibleDepartment.Operations, projection?.Department);
        });
    }

    [Fact]
    public async Task A_third_country_national_cannot_skip_legalisation()
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

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/workers/{worker.WorkerId}/status",
            WorkerTestData.WorkerStatusRequest(WorkerStatus.Onboarding)
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
