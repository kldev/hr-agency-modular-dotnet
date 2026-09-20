using System.Net;
using System.Net.Http.Json;
using HrAgencySystem.Api.Endpoints.Project.Maps;
using HrAgencySystem.IntegrationTests.Infrastructure;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.Compliance;
using Xunit.Abstractions;

namespace HrAgencySystem.IntegrationTests.Projects;

[Collection(IntegrationCollection.Name)]
public class ProjectComplianceTests(IntegrationEnvironment env, ITestOutputHelper outputHelper)
    : BaseIntegrationTest(env, outputHelper)
{
    protected override async Task BeforeEachAsync()
    {
        await Cleaner.CleanProjects();
        await Cleaner.CleanCompany();
    }

    [Fact]
    public async Task Belgian_agency_work_lists_the_belgian_requirements()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "be");

        var catalogue = await GetCatalogue(organizationId, project);

        var requirements = catalogue.Select(c => c.Requirement).ToList();
        Assert.Contains(ComplianceRequirement.BeLimosaDeclaration, requirements);
        Assert.Contains(ComplianceRequirement.BeLiaisonPerson, requirements);
        Assert.Contains(ComplianceRequirement.BeTemporaryAgencyRecognition, requirements);
        Assert.Contains(ComplianceRequirement.BeUserJointCommittee, requirements);
        Assert.DoesNotContain(ComplianceRequirement.DeAuegPermit, requirements);

        // Nothing recorded yet, so every row is an empty one rather than an absent one.
        Assert.All(catalogue, c => Assert.Null(c.Item));
    }

    [Fact]
    public async Task German_agency_work_lists_the_german_requirements()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "de");

        var requirements = (await GetCatalogue(organizationId, project))
            .Select(c => c.Requirement)
            .ToList();

        Assert.Contains(ComplianceRequirement.DeAuegPermit, requirements);
        Assert.Contains(ComplianceRequirement.DeAuegNotification, requirements);
        Assert.Contains(ComplianceRequirement.DeUeberlassungAgreement, requirements);
        Assert.DoesNotContain(ComplianceRequirement.BeLimosaDeclaration, requirements);
    }

    [Fact]
    public async Task A_polish_project_has_an_empty_catalogue()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "pl");

        Assert.Empty(await GetCatalogue(organizationId, project));
    }

    [Fact]
    public async Task Recording_a_requirement_settles_it_and_shows_the_next_expiry()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "be");

        // The projection runs in the async daemon, so the starting count has to be waited for.
        var outstandingBefore = 0;
        await Eventually.AssertAsync(async () =>
        {
            var before = await ProjectClient.GetAsync(organizationId, project);

            Assert.NotNull(before);
            outstandingBefore = before.ComplianceOutstandingCount;
            Assert.True(outstandingBefore > 0);
        });

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project}/compliance/{ComplianceRequirement.BeLimosaDeclaration}",
            new MapRecordCompliance.RecordComplianceItemRequest(
                ComplianceStatus.Confirmed,
                "L1-2026-0001",
                new DateOnly(2026, 10, 1),
                new DateOnly(2027, 9, 30),
                null,
                null
            )
        );
        response.EnsureSuccessStatusCode();

        await Eventually.AssertAsync(async () =>
        {
            var projection = await ProjectClient.GetAsync(organizationId, project);

            Assert.NotNull(projection);
            Assert.Equal(outstandingBefore - 1, projection.ComplianceOutstandingCount);

            // The nearest expiry is what a list has to show so that a lapsing declaration is seen
            // before it lapses, not after.
            Assert.Equal(new DateOnly(2027, 9, 30), projection.NextComplianceExpiryOn);
        });
    }

    [Fact]
    public async Task Recording_a_requirement_that_does_not_apply_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "be");

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project}/compliance/{ComplianceRequirement.DeAuegPermit}",
            new MapRecordCompliance.RecordComplianceItemRequest(
                ComplianceStatus.Confirmed,
                "AUG-1",
                null,
                null,
                null,
                null
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Confirming_a_numbered_requirement_without_its_number_is_refused()
    {
        var organizationId = Guid.NewGuid();
        var project = await CreateProject(organizationId, "be");

        Client.WithOrganizationId(organizationId);
        var response = await Client.PutAsJsonAsync(
            $"/api/projects/{project}/compliance/{ComplianceRequirement.BeLimosaDeclaration}",
            new MapRecordCompliance.RecordComplianceItemRequest(
                ComplianceStatus.Confirmed,
                null,
                null,
                null,
                null,
                null
            )
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> CreateProject(Guid organizationId, string countryCode)
    {
        var companyId = await ProjectClient.CreateCompanyAsync(organizationId);
        var project = await ProjectClient.CreateAsync(
            organizationId,
            companyId,
            EngagementType.TemporaryAgencyWork,
            countryCode
        );

        return project.ProjectId;
    }

    private async Task<
        IReadOnlyList<MapComplianceCatalogue.ComplianceRequirementView>
    > GetCatalogue(Guid organizationId, Guid projectId)
    {
        Client.WithOrganizationId(organizationId);

        IReadOnlyList<MapComplianceCatalogue.ComplianceRequirementView> catalogue = [];

        await Eventually.AssertAsync(async () =>
        {
            var response = await Client.GetAsync(
                $"/api/projects/{projectId}/compliance/catalogue"
            );
            response.EnsureSuccessStatusCode();

            catalogue =
                await response.ReadWithJson<
                    List<MapComplianceCatalogue.ComplianceRequirementView>
                >() ?? [];
        });

        return catalogue;
    }
}
