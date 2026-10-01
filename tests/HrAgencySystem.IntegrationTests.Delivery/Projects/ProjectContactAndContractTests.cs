using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Application.Contacts.Remove;
using HrAgencySystem.Projects.Domain;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class ProjectContactAndContractTests(
    IntegrationEnvironment env,
    ITestOutputHelper outputHelper
) : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task Assigning_a_responsible_contact_shows_up_flattened_on_the_read_model()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal("Marie", projection.ResponsibleContact!.FirstName);
            Assert.Single(projection.Contacts);
        });
    }

    [Fact]
    public async Task Changing_the_responsible_contact_replaces_rather_than_adds()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible
        );
        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible,
            "Luc",
            "luc@acme.example.com"
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Single(projection.Contacts);
            Assert.Equal("Luc", projection.ResponsibleContact!.FirstName);
        });
    }

    [Fact]
    public async Task The_signatory_and_the_responsible_contact_are_independent()
    {
        // Who signed is a historical fact; who answers today is not. Changing one must never
        // quietly change the other.
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.ContractSignatory,
            "Paul",
            "paul@acme.example.com"
        );
        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible
        );
        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.Responsible,
            "Luc",
            "luc@acme.example.com"
        );

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal(2, projection.Contacts.Count);
            Assert.Equal(
                "Paul",
                projection
                    .Contacts.Single(c => c.Role == ContactRole.ContractSignatory)
                    .Person.FirstName
            );
            Assert.Equal("Luc", projection.ResponsibleContact!.FirstName);
        });
    }

    [Fact]
    public async Task A_live_project_cannot_lose_its_responsible_contact()
    {
        var organizationId = Guid.NewGuid();
        var project = await ProjectClient.CreateReadyToGoLiveAsync(organizationId);
        await ProjectClient.ChangeStatusAsync(
            organizationId,
            project.ProjectId,
            ProjectStatus.Active
        );

        Client.WithOrganizationId(organizationId);
        var response = await Client.DeleteAsync(
            $"/api/projects/{project.ProjectId}/contacts/{ContactRole.Responsible}"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.ReadWithJson<ProblemDetails>(OutputHelper);
        Assert.NotNull(problem);
        Assert.Equal(
            RemoveProjectContactHandler.ResponsibleRequiredWhileLiveMessage,
            problem.Detail
        );
    }

    [Fact]
    public async Task Recording_a_contract_freezes_the_client_as_it_is_now()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyWithProfileAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        await ProjectClient.AssignContactAsync(
            organizationId,
            project.ProjectId,
            ContactRole.ContractSignatory,
            "Paul",
            "paul@acme.example.com"
        );
        await ProjectClient.RecordContractAsync(organizationId, project.ProjectId);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.NotNull(projection.Contract);
            Assert.Equal("UM/2026/17", projection.Contract!.ContractNumber);
            Assert.Equal(ContractStatus.Signed, projection.Contract.Status);
            Assert.Equal(ProjectTestData.SignedOn, projection.Contract.SignedOn);

            // The party is a copy, not a pointer.
            Assert.Equal("ACME Corporation sp. z o.o.", projection.Contract.Party.LegalName);
            Assert.Equal("Warszawa", projection.Contract.Party.RegisteredAddress.City);

            // The signatory is taken from whoever holds that role at the moment of recording.
            Assert.Equal("Paul", projection.Contract.SignedBy!.FirstName);
        });
    }

    [Fact]
    public async Task Terminating_a_contract_keeps_its_signature_date()
    {
        var organizationId = Guid.NewGuid();
        var project = await ProjectClient.CreateReadyToGoLiveAsync(organizationId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}/contract/status",
            new MapChangeContractStatus.ChangeContractStatusRequest(ContractStatus.Terminated, null)
        );
        response.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal(ContractStatus.Terminated, projection.Contract!.Status);
            Assert.Equal(ProjectTestData.SignedOn, projection.Contract.SignedOn);
        });
    }

    [Fact]
    public async Task Invoice_and_document_addresses_are_kept_apart()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        Client.WithOrganizationId(organizationId);

        await SetEmails(
            project.ProjectId,
            EmailPurpose.Invoice,
            ["ap@acme.example.com", "finance@acme.example.com"]
        );
        await SetEmails(project.ProjectId, EmailPurpose.Document, ["docs@acme.example.com"]);
        await SetEmails(project.ProjectId, EmailPurpose.Invoice, ["billing@acme.example.com"]);

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project.ProjectId);

            Assert.NotNull(projection);
            Assert.Equal(2, projection.EmailRecipients.Count);
            Assert.Contains(
                projection.EmailRecipients,
                r => r.Purpose == EmailPurpose.Invoice && r.Email == "billing@acme.example.com"
            );
            Assert.Contains(
                projection.EmailRecipients,
                r => r.Purpose == EmailPurpose.Document && r.Email == "docs@acme.example.com"
            );
        });
    }

    [Fact]
    public async Task A_bad_email_address_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(organizationId, companyId);

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project.ProjectId}/emails/{EmailPurpose.Invoice}",
            new MapSetEmails.SetProjectEmailRecipientsRequest(["nope"])
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SetEmails(Guid projectId, EmailPurpose purpose, string[] emails)
    {
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{projectId}/emails/{purpose}",
            new MapSetEmails.SetProjectEmailRecipientsRequest(emails)
        );

        response.EnsureSuccessStatusCode();
    }
}
