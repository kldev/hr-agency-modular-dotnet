using System.Net;
using System.Net.Http.Json;
using System.Text;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.Compliance;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Application.Documents.Remove;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class ProjectDocumentTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    private static readonly DateOnly DocumentDate = new(2026, 9, 10);

    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task Attaching_a_document_records_its_metadata_and_a_file_reference()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);

        var attached = await Attach(organizationId, project);

        Assert.Equal(DocumentCategory.Contract, attached.Document.Category);
        Assert.Equal("umowa.pdf", attached.Document.FileName);
        Assert.Equal(DocumentDate, attached.Document.DocumentDate);
        Assert.NotEqual(Guid.Empty, attached.Document.FileId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project);

            Assert.NotNull(projection);
            Assert.Equal(1, projection.DocumentCount);
            Assert.Equal("umowa.pdf", projection.Documents[0].FileName);
        });
    }

    [Fact]
    public async Task A_document_may_carry_an_expiry_or_none_at_all()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);

        var withoutExpiry = await Attach(organizationId, project);
        var withExpiry = await Attach(
            organizationId,
            project,
            category: DocumentCategory.Insurance,
            validUntil: new DateOnly(2027, 9, 10)
        );

        Assert.Null(withoutExpiry.Document.ValidUntil);
        Assert.Equal(new DateOnly(2027, 9, 10), withExpiry.Document.ValidUntil);
    }

    [Fact]
    public async Task A_document_cannot_expire_before_it_was_issued()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);

        var response = await Upload(organizationId, project, validUntil: DocumentDate.AddDays(-1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_file_can_be_read_back_through_the_api()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);
        var attached = await Attach(organizationId, project);

        await Eventually.AssertAsync(async () =>
        {
            Client.WithOrganizationId(organizationId);

            var response = await Client.GetAsync(
                $"/api/projects/{project}/documents/{attached.Document.DocumentId}/content"
            );
            response.EnsureSuccessStatusCode();

            Assert.Equal("the signed contract", await response.Content.ReadAsStringAsync());
        });
    }

    [Fact]
    public async Task Another_organization_cannot_read_the_file()
    {
        var owner = Guid.NewGuid();
        var project = await CreateProject(owner);
        var attached = await Attach(owner, project);

        Client.WithOrganizationId(Guid.NewGuid());

        var response = await Client.GetAsync(
            $"/api/projects/{project}/documents/{attached.Document.DocumentId}/content"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Another_organization_cannot_attach_a_document()
    {
        var owner = Guid.NewGuid();
        var project = await CreateProject(owner);

        var response = await Upload(Guid.NewGuid(), project);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Updating_metadata_leaves_the_file_alone()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);
        var attached = await Attach(organizationId, project);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project}/documents/{attached.Document.DocumentId}",
            new MapUpdateDocument.UpdateProjectDocumentRequest(
                DocumentCategory.Annex,
                DocumentDate,
                new DateOnly(2028, 1, 1),
                "Signed annex one."
            )
        );
        response.EnsureSuccessStatusCode();

        var changed = await response.ReadWithJson<ProjectDocumentMetadataChanged>(OutputHelper);

        Assert.NotNull(changed);
        Assert.Equal(DocumentCategory.Annex, changed.Document.Category);
        Assert.Equal(attached.Document.FileId, changed.Document.FileId);
    }

    [Fact]
    public async Task Removing_a_document_takes_it_off_the_project()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);
        var attached = await Attach(organizationId, project);

        Client.WithOrganizationId(organizationId);
        var response = await Client.DeleteAsync(
            $"/api/projects/{project}/documents/{attached.Document.DocumentId}"
        );
        response.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project);

            Assert.NotNull(projection);
            Assert.Equal(0, projection.DocumentCount);
        });
    }

    [Fact]
    public async Task A_document_recorded_as_compliance_proof_cannot_be_removed()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId);
        var attached = await Attach(organizationId, project, DocumentCategory.Compliance);

        Client.WithOrganizationId(organizationId);
        var recorded = await Client.PutAsJsonAsync(
            $"/api/projects/{project}/compliance/{ComplianceRequirement.BeJointCommittee}",
            new MapRecordCompliance.RecordComplianceItemRequest(
                ComplianceStatus.Confirmed,
                "PC 124.00",
                null,
                null,
                attached.Document.DocumentId,
                null
            )
        );
        recorded.EnsureSuccessStatusCode();

        var response = await Client.DeleteAsync(
            $"/api/projects/{project}/documents/{attached.Document.DocumentId}"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.NotNull(problem);
        Assert.Equal(RemoveProjectDocumentHandler.ReferencedByComplianceMessage, problem.Detail);
    }

    private async Task<Guid> CreateProject(Guid organizationId)
    {
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        return project.ProjectId;
    }

    private async Task<ProjectDocumentAttached> Attach(
        Guid organizationId,
        Guid projectId,
        DocumentCategory category = DocumentCategory.Contract,
        DateOnly? validUntil = null
    )
    {
        var response = await Upload(organizationId, projectId, category, validUntil);
        response.EnsureSuccessStatusCode();

        var attached = await response.ReadWithJson<ProjectDocumentAttached>(OutputHelper);
        Assert.NotNull(attached);

        return attached;
    }

    private async Task<HttpResponseMessage> Upload(
        Guid organizationId,
        Guid projectId,
        DocumentCategory category = DocumentCategory.Contract,
        DateOnly? validUntil = null
    )
    {
        Client.WithOrganizationId(organizationId);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("the signed contract"));
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/pdf"
        );

        form.Add(file, "file", "umowa.pdf");
        form.Add(new StringContent(category.ToString()), "category");
        form.Add(new StringContent(DocumentDate.ToString("O")), "documentDate");
        if (validUntil is not null)
            form.Add(new StringContent(validUntil.Value.ToString("O")), "validUntil");

        return await Client.PostAsync($"/api/projects/{projectId}/documents", form);
    }
}
