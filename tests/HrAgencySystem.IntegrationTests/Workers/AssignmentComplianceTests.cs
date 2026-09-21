using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using HrAgencySystem.Api.Endpoints.Assignment.Maps;
using HrAgencySystem.Compliance;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.IntegrationTests.Projects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Workers;

/// <summary>
/// The register CLAUDE.md said did not exist: A1 answered per person per posting, rather than one
/// tick on a project claiming everybody is covered.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class AssignmentComplianceTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
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
    public async Task The_catalogue_lists_what_this_person_needs_for_this_posting()
    {
        var (organizationId, assignmentId) = await PostingAsync();

        await Eventually.AssertAsync(async () =>
        {
            var catalogue = await CatalogueAsync(organizationId, assignmentId);

            Assert.Contains(catalogue, r => r.Requirement == ComplianceRequirement.A1Certificates);

            // An untouched requirement is an empty row, not an absence.
            Assert.All(catalogue, r => Assert.Null(r.Item));

            // The agency's own obligations are not on this list; they are the project's.
            Assert.DoesNotContain(
                catalogue,
                r => r.Requirement == ComplianceRequirement.BeTemporaryAgencyRecognition
            );
        });
    }

    [Fact]
    public async Task Two_people_on_one_project_each_answer_for_their_own_A1()
    {
        var organizationId = Guid.NewGuid();
        var projectId = await NewProjectAsync(organizationId);

        var first = await PlanForNewWorkerAsync(organizationId, projectId, "Jan", "Kowalski");
        var second = await PlanForNewWorkerAsync(organizationId, projectId, "Anna", "Nowak");

        await RecordAsync(organizationId, first, ComplianceRequirement.A1Certificates);

        await Eventually.AssertAsync(async () =>
        {
            var covered = await WorkerClient.GetAssignmentAsync(organizationId, first);
            var uncovered = await WorkerClient.GetAssignmentAsync(organizationId, second);

            Assert.NotNull(covered);
            Assert.NotNull(uncovered);

            // One is settled and the other is not, which is a thing a single project level tick
            // could never say.
            Assert.Contains(
                covered.Compliance,
                c => c.Requirement == ComplianceRequirement.A1Certificates && c.IsSettled
            );
            Assert.Empty(uncovered.Compliance);
            Assert.True(uncovered.ComplianceOutstandingCount > covered.ComplianceOutstandingCount);
        });
    }

    [Fact]
    public async Task An_A1_cannot_be_recorded_against_the_project_any_more()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyWithProfileAsync(organizationId);
        var project = await ProjectClient.CreateAsync(
            organizationId,
            companyId,
            EngagementType.PostingOfWorkers
        );

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}/compliance/{ComplianceRequirement.A1Certificates}",
            new { Status = ComplianceStatus.Confirmed }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.Contains("belongs to a worker's assignment", problem?.Detail ?? "");
    }

    [Fact]
    public async Task A_document_recorded_as_proof_cannot_be_removed()
    {
        var (organizationId, assignmentId) = await PostingAsync();

        var attached = await AttachAsync(organizationId, assignmentId);
        await RecordAsync(
            organizationId,
            assignmentId,
            ComplianceRequirement.A1Certificates,
            attached.Document.DocumentId
        );

        Client.WithOrganizationId(organizationId);
        var response = await Client.DeleteAsync(
            $"/api/assignments/{assignmentId}/documents/{attached.Document.DocumentId}"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Local_employment_asks_for_a_local_contract_and_never_for_an_A1()
    {
        var organizationId = Guid.NewGuid();
        var projectId = await NewProjectAsync(organizationId);
        var workerId = await WorkerClient.EmployedAsync(organizationId);

        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            projectId.ProjectId,
            projectId.PositionId,
            EngagementType.LocalEmployment
        );

        await Eventually.AssertAsync(async () =>
        {
            var catalogue = await CatalogueAsync(organizationId, planned.AssignmentId);

            Assert.DoesNotContain(
                catalogue,
                r => r.Requirement == ComplianceRequirement.A1Certificates
            );
            Assert.Contains(
                catalogue,
                r => r.Requirement == ComplianceRequirement.LocalEmploymentContract
            );
            Assert.Contains(
                catalogue,
                r => r.Requirement == ComplianceRequirement.BeDimonaDeclaration
            );
        });
    }

    private async Task<List<MapComplianceCatalogue.ComplianceRequirementView>> CatalogueAsync(
        Guid organizationId,
        Guid assignmentId
    )
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.GetAsync(
            $"/api/assignments/{assignmentId}/compliance/catalogue"
        );
        response.EnsureSuccessStatusCode();

        var catalogue = await response.ReadWithJson<
            List<MapComplianceCatalogue.ComplianceRequirementView>
        >(OutputHelper);

        Assert.NotNull(catalogue);

        return catalogue;
    }

    private async Task<(Guid OrganizationId, Guid AssignmentId)> PostingAsync()
    {
        var organizationId = Guid.NewGuid();
        var projectId = await NewProjectAsync(organizationId);
        var workerId = await WorkerClient.EmployedAsync(organizationId);
        var planned = await WorkerClient.PlanAsync(
            organizationId,
            workerId,
            projectId.ProjectId,
            projectId.PositionId
        );

        return (organizationId, planned.AssignmentId);
    }

    private async Task<Guid> PlanForNewWorkerAsync(
        Guid organizationId,
        ProjectTestClient.SeededDelivery delivery,
        string firstName,
        string lastName
    )
    {
        var worker = await WorkerClient.RegisterAsync(
            organizationId,
            firstName: firstName,
            lastName: lastName
        );
        var planned = await WorkerClient.PlanAsync(
            organizationId,
            worker.WorkerId,
            delivery.ProjectId,
            delivery.PositionId
        );

        return planned.AssignmentId;
    }

    private async Task RecordAsync(
        Guid organizationId,
        Guid assignmentId,
        ComplianceRequirement requirement,
        Guid? documentId = null
    )
    {
        Client.WithOrganizationId(organizationId);

        var response = await Client.PutAsJsonAsync(
            $"/api/assignments/{assignmentId}/compliance/{requirement}",
            WorkerTestData.ComplianceRequest(documentId: documentId)
        );
        response.EnsureSuccessStatusCode();
    }

    private async Task<AssignmentDocumentAttached> AttachAsync(
        Guid organizationId,
        Guid assignmentId
    )
    {
        Client.WithOrganizationId(organizationId);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("the A1 certificate"));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "file", "a1.pdf");
        form.Add(
            new StringContent(AssignmentDocumentCategory.SocialSecurity.ToString()),
            "category"
        );
        form.Add(new StringContent(new DateOnly(2026, 9, 15).ToString("O")), "documentDate");

        var response = await Client.PostAsync($"/api/assignments/{assignmentId}/documents", form);
        response.EnsureSuccessStatusCode();

        var attached = await response.ReadWithJson<AssignmentDocumentAttached>(OutputHelper);
        Assert.NotNull(attached);

        return attached;
    }

    private Task<ProjectTestClient.SeededDelivery> NewProjectAsync(Guid organizationId) =>
        ProjectClient.CreateWithPositionAsync(organizationId);
}
